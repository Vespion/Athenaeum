using System.Reflection;
using System.Runtime.Versioning;
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
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public interface IPluginResolutionService
{
	Task<IEnumerable<string>> ResolvePluginsAsync(IProgress<string> progress);
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
		_logger.LogDebug("Loading default settings with no root");
		var settings = Settings.LoadDefaultSettings(null);

		// foreach (var feed in _options.Value.Feeds)
		// {
		// 	settings.AddOrUpdate("packageSources", new ClearItem());
		// 	settings.AddOrUpdate("packageSources", new AddItem(feed.Name, feed.Url));
		// }
		
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
	public async Task<IEnumerable<string>> ResolvePluginsAsync(IProgress<string> progress)
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
		
		var assemblyList = new List<string>();
		
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
				
				progress.Report($"Resolving assembly for {package.Id}...");

				IEnumerable<string> GetAssemblies(IReadOnlyCollection<FrameworkSpecificGroup> frameworkSpecificGroups)
				{
					var nearest = frameworkReducer.GetNearest(nuGetFramework,
						frameworkSpecificGroups.Select(x => x.TargetFramework));
					
					foreach (var frameworkSpecificGroup in frameworkSpecificGroups)
					{
						if (frameworkSpecificGroup.TargetFramework == nearest)
						{
							foreach (var item in frameworkSpecificGroup.Items)
							{
								if (Path.GetExtension(item).ToLower() == ".dll")
								{
									yield return Path.Combine(_options.Value.PluginDirectory, $"{package.Id}.{package.Version}", item);
								}
							}
						}
					}
				}
				
				var libItems = packageReader.GetLibItems().ToArray();
				assemblyList.AddRange(GetAssemblies(libItems));
				
				var frameworkItems = packageReader.GetFrameworkItems().ToArray();
				assemblyList.AddRange(GetAssemblies(frameworkItems));
			}
		}

		return assemblyList;
	}
}