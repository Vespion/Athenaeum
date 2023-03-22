using System.Reflection;
using System.Runtime.Versioning;
using FluentScanning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using VespionSoftworks.Athenaeum.Plugins.Abstractions;
using VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public interface IPluginResolutionService
{
	IAsyncEnumerable<PluginPackage> ResolvePluginsAsync(IProgress<string> progress);
}

public class PluginResolutionService: IPluginResolutionService
{
	private readonly IOptions<PluginConfiguration> _options;
	private readonly NuGet.Common.ILogger _nugetLogger;
	private readonly ILogger<PluginResolutionService> _logger;

	public PluginResolutionService(IOptions<PluginConfiguration> options, ILogger<PluginResolutionService> logger, NuGet.Common.ILogger nugetLogger)
	{
		_options = options;
		_logger = logger;
		_nugetLogger = nugetLogger;
	}

	private ISettings GetSettingsFromPluginConfig()
	{
		var settingsFilePath = Path.GetTempFileName();
		_logger.LogDebug("Generating temporary empty settings @ '{Path}'", settingsFilePath);


		//Write out a basic settings file, it's annoying to have to do this but it's the only way to get the settings to load
		File.WriteAllText(settingsFilePath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  
</configuration>");
		
		var settings = Settings.LoadDefaultSettings(Path.GetDirectoryName(settingsFilePath), Path.GetFileName(settingsFilePath), null);
		
		return settings;
	}
	
	private async Task GetPackageDependencies(PackageIdentity package,
		NuGetFramework framework,
		SourceCacheContext cacheContext,
		NuGet.Common.ILogger logger,
		ICollection<SourceRepository> repositories,
		ISet<SourcePackageDependencyInfo> availablePackages)
	{
		if (availablePackages.Contains(package)) return;

		foreach (var sourceRepository in repositories)
		{
			var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
			var dependencyInfo = await dependencyInfoResource.ResolvePackage(
				package, framework, cacheContext, logger, CancellationToken.None);

			if (dependencyInfo == null) continue;

			availablePackages.Add(dependencyInfo);
			foreach (var dependency in dependencyInfo.Dependencies)
			{
				await GetPackageDependencies(
					new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
					framework, cacheContext, logger, repositories, availablePackages);
			}
		}
	}
	
	private async Task<HashSet<SourcePackageDependencyInfo>> ResolvePlugin(NuGetPackage plugin,
		SourceCacheContext cacheContext, SourceRepositoryProvider sourceRepositoryProvider, NuGetFramework nuGetFramework)
	{
		var repositories = sourceRepositoryProvider.GetRepositories().ToArray();
		var availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
		
		await GetPackageDependencies(
			new PackageIdentity(plugin.Name, new NuGetVersion(plugin.Version)),
			nuGetFramework,
			cacheContext,
			_nugetLogger,
			repositories,
			availablePackages
		);
		
		return availablePackages;
	}

	private IEnumerable<SourcePackageDependencyInfo> ResolveDependencyGraph(string packageId, ISet<SourcePackageDependencyInfo> availablePackages, ISourceRepositoryProvider sourceRepositoryProvider)
	{
		var resolverContext = new PackageResolverContext(
			DependencyBehavior.Lowest,
			new[] { packageId },
			Enumerable.Empty<string>(),
			Enumerable.Empty<PackageReference>(),
			Enumerable.Empty<PackageIdentity>(),
			availablePackages,
			sourceRepositoryProvider.GetRepositories().Select(s => s.PackageSource),
			_nugetLogger);

		var resolver = new PackageResolver();
		var packagesToInstall = resolver.Resolve(resolverContext, CancellationToken.None)
			.Select(p => availablePackages.Single(x => PackageIdentityComparer.Default.Equals(x, p)));

		return packagesToInstall;
	}

	private async Task<DownloadResourceResult> DownloadPackageAsync(SourcePackageDependencyInfo packageToInstall, SourceCacheContext cacheContext, ISettings settings)
	{
		var downloadResource = await packageToInstall.Source.GetResourceAsync<DownloadResource>(CancellationToken.None);
		var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
			packageToInstall,
			new PackageDownloadContext(cacheContext),
			SettingsUtility.GetGlobalPackagesFolder(settings),
			_nugetLogger, CancellationToken.None);

		return downloadResult;
	}
	
	/// <inheritdoc />
	public async IAsyncEnumerable<PluginPackage> ResolvePluginsAsync(IProgress<string> progress)
	{
		progress.Report("Loading NuGet...");
		var settings = GetSettingsFromPluginConfig();
		using var cacheContext = new SourceCacheContext();
		
		var sources = _options.Value.Feeds
			.Select(x => new PackageSource(x.Url, x.Name))
			.ToArray();
		var packageSourceProvider = new PackageSourceProvider(settings, sources);
		var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
		
		var targetFramework = Assembly
			.GetEntryAssembly()!
			.GetCustomAttribute<TargetFrameworkAttribute>()!
			.FrameworkName;
		
		var frameworkNameProvider = new FrameworkNameProvider(
			new[] { DefaultFrameworkMappings.Instance },
			new[] { DefaultPortableFrameworkMappings.Instance });

		var frameworkReducer = new FrameworkReducer();
		var nuGetFramework = NuGetFramework.ParseFrameworkName(targetFramework, frameworkNameProvider);
		
		var packagePathResolver = new PackagePathResolver(Path.GetFullPath(_options.Value.PluginDirectory));
		var packageExtractionContext = new PackageExtractionContext(
			PackageSaveMode.Defaultv3,
			XmlDocFileSaveMode.None,
			ClientPolicyContext.GetClientPolicy(settings, _nugetLogger), 
			_nugetLogger
		);
		
		foreach (var plugin in _options.Value.Plugins)
		{

			progress.Report($"Resolving plugin {plugin.Name}...");
			var resolvedPackages = await ResolvePlugin(plugin, cacheContext, sourceRepositoryProvider, nuGetFramework);
			progress.Report($"Resolving dependencies for plugin {plugin.Name}...");
			var packagesToInstall = ResolveDependencyGraph(plugin.Name, resolvedPackages, sourceRepositoryProvider);
			
			foreach (var package in packagesToInstall)
			{
				PackageReaderBase packageReader;
				var installedPath = packagePathResolver.GetInstalledPath(package);
				if (installedPath == null)
				{
					// Install packages
					progress.Report($"Downloading {package.Id}...");
					var downloadResult = await DownloadPackageAsync(package, cacheContext, settings);
					progress.Report($"Extracting {package.Id}...");
				
					await PackageExtractor.ExtractPackageAsync(
						downloadResult.PackageSource,
						downloadResult.PackageStream,
						packagePathResolver,
						packageExtractionContext,
						CancellationToken.None);
					
					packageReader = downloadResult.PackageReader;
				}
				else
				{
					packageReader = new PackageFolderReader(installedPath);
				}

				var tags = packageReader.NuspecReader.GetTags().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				if (!(tags.Contains("athenaeum") && tags.Contains("plugin")))
				{
					continue;
				}

				var header = new PluginHeader(package.Version, await packageReader.GetPrimarySignatureAsync(default));
				
				progress.Report($"Resolving assembly for {package.Id}...");
				var pluginPackage = new PluginPackage(
					header
				);

				IEnumerable<string> GetItems(IReadOnlyCollection<FrameworkSpecificGroup> frameworkSpecificGroups, string ext = ".dll")
				{
					var nearest = frameworkReducer.GetNearest(nuGetFramework,
						frameworkSpecificGroups.Select(x => x.TargetFramework));
					
					foreach (var frameworkSpecificGroup in frameworkSpecificGroups)
					{
						if (frameworkSpecificGroup.TargetFramework == nearest)
						{
							foreach (var item in frameworkSpecificGroup.Items)
							{
								if (Path.GetExtension(item).ToLower() == ext)
								{
									yield return Path.Combine(_options.Value.PluginDirectory, $"{package.Id}.{package.Version}", item);
								}
							}
						}
					}
				}

				void ScanForPlugins(IEnumerable<string> assemblyPaths)
				{
					var assemblies = assemblyPaths
						.Select(s =>
						{
							Assembly Func() => Assembly.LoadFrom(s);
							return (AssemblyProvider) (Func<Assembly>)Func;
						})
						.ToArray();
					
					var scanner = new AssemblyScanner(assemblies);

					var bootstraps = scanner.ScanForTypesThat()
						.AreAssignableTo<IPluginBootstrapper>()
						.AreClasses()
						.ToArray();
					
					var storage = new List<Type>(
						scanner.ScanForTypesThat()
							.AreAssignableTo<IStoragePlugin>()
							.AreClasses()
							.ToArray()
					);
					
					storage.AddRange(
						scanner.ScanForTypesThat()
							.AreAssignableTo<IAuthenticatedStoragePlugin>()
							.AreClasses()
							.ToArray()
					);

					var storageFactory = scanner.ScanForTypesThat()
						.AreAssignableTo<IStorageFactoryPlugin>()
						.AreClasses()
						.ToArray();

					pluginPackage.Bootstrappers = bootstraps;
					pluginPackage.StoragePlugins = storage;
					pluginPackage.StorageFactoryPlugins = storageFactory;

					 var infoProvider = scanner.ScanForTypesThat()
						.AreAssignableTo<IPluginInfoProvider>()
						.ToArray();
					 
					 if (infoProvider == default || infoProvider.Length == 0)
					 {
						 throw new InvalidProgramException("No info provider found");
					 }
					 
					 pluginPackage.InfoProvider = infoProvider[0];
				}
				
				var libItems = packageReader.GetLibItems().ToArray();
				var frameworkItems = packageReader.GetFrameworkItems().ToArray();

				try
				{
					ScanForPlugins(GetItems(libItems).Concat(GetItems(frameworkItems)));
				}
				catch (InvalidProgramException ex) when(ex.Message == "No info provider found")
				{
					//This is an invalid plugin but we can still continue loading others
					continue;
				}
				yield return pluginPackage;
			}
		}
	}
}