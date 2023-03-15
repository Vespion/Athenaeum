using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NuGet.Common;
using NuGet.ProjectManagement;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public static class ServiceExtensions
{
	public static IServiceCollection AddPluginServices(this IServiceCollection services, HostBuilderContext ctx)
	{
		services.Configure<PluginConfiguration>(ctx.Configuration.GetSection("Plugins"));
		services.AddOptions<PluginConfiguration>()
			.ValidateDataAnnotations();

		services.AddTransient<INuGetProjectContext, ProjectContext>();
		services.AddTransient<PluginResolutionService>();
		services.AddTransient<ILogger, NugetLogger>();
		
		return services;
	}
}