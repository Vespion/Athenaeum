using Microsoft.Extensions.Options;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public class PluginResolutionService
{
	private readonly IOptions<PluginConfiguration> _options;
	private readonly INuGetProjectContext _projectContext;

	public PluginResolutionService(IOptions<PluginConfiguration> options, INuGetProjectContext projectContext)
	{
		_options = options;
		_projectContext = projectContext;
	}

	// public IEnumerable<Assembly> LoadPluginAssemblies()
	// {
	// 	
	// }

	public async ValueTask ResolvePluginsAsync()
	{
		var pluginFolder = Path.GetFullPath(_options.Value.PluginDirectory);

		var sources = _options.Value.Feeds
			.Select(x => new PackageSource(x.Url, x.Name))
			.ToArray();
		var packageSourceProvider = new PackageSourceProvider(new NullSettings(), sources);
		var sourceProvider = new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
			
		var project = new FolderNuGetProject(pluginFolder);
		var packageManager = new NuGetPackageManager(
			sourceProvider,
			new NullSettings(),
			Path.Combine(pluginFolder, ".pkgs")
		)
		{
			PackagesFolderNuGetProject = project
		};
			
		var resolutionContext = new ResolutionContext(
			DependencyBehavior.Lowest, false, false, VersionConstraints.None);
			
		foreach (var nuGetPackage in _options.Value.Plugins)
		{
			await packageManager.InstallPackageAsync(
				packageManager.PackagesFolderNuGetProject,
				nuGetPackage,
				resolutionContext,
				_projectContext,
				sourceProvider.GetRepositories(),
				Array.Empty<SourceRepository>(),  // This is a list of secondary source repositories, probably empty
				CancellationToken.None
			);
		}
	}
}