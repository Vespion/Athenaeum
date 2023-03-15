using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NuGet.Common;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public static class ServiceExtensions
{
	public static IServiceCollection AddPluginServices(this IServiceCollection services, HostBuilderContext ctx)
	{
		services.Configure<PluginConfiguration>(ctx.Configuration.GetSection("Plugins"));
		services.AddOptions<PluginConfiguration>()
			.ValidateDataAnnotations();

		services.AddTransient<IPluginResolutionService, PluginResolutionService>();
		services.AddTransient<ILogger, NugetLogger>();
		
		return services;
	}
}