using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VespionSoftworks.Athenaeum.Plugins.Abstractions;
using VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public static class ServiceExtensions
{
	public static IServiceCollection AddPluginServices(this IServiceCollection services, IConfiguration ctx)
	{
		services.Configure<PluginConfiguration>(ctx.GetSection("Plugins"));
		services.AddOptions<PluginConfiguration>()
			.ValidateDataAnnotations();

		services.TryAddTransient<IPluginResolutionService, PluginResolutionService>();
		services.TryAddTransient<NuGet.Common.ILogger, NugetLogger>();
		services.TryAddScoped<IPluginPackageAccessor, PluginPackageAccessor>();
		
		return services;
	}

	public static IServiceCollection BootstrapPlugins(this IServiceCollection x)
	{
		using (var bootstrapProvider = x.BuildServiceProvider())
		{
			var bootstrappers = bootstrapProvider.GetServices<IPluginBootstrapper>();
			foreach (var bootstrapper in bootstrappers)
			{
				bootstrapper.ConfigureServices(x);
			}
		}

		return x;
	}
	
	public static IServiceCollection ScanForPlugins(this IServiceCollection x, IProgress<string>? progress = null)
	{
		using (var pluginProvider = x.BuildServiceProvider())
		{
			var resolver = pluginProvider.GetRequiredService<IPluginResolutionService>();
        			
			var pluginProgress = new Progress<string>(s => progress?.Report(s));
			var packages = resolver.ResolvePluginsAsync(pluginProgress).ToBlockingEnumerable();
				
			foreach (var pluginPackage in packages)
			{
				foreach (var storagePlugin in pluginPackage.StoragePlugins)
				{
					x.AddScoped(typeof(IStoragePlugin), storagePlugin);
					// ReSharper disable once SuspiciousTypeConversion.Global
					if (storagePlugin is IAuthenticatedStoragePlugin)
					{
						x.AddScoped(typeof(IAuthenticatedStoragePlugin), storagePlugin);

					}
				}
				
				foreach (var bootstrapper in pluginPackage.Bootstrappers)
				{
					x.AddTransient(typeof(IStorageFactoryPlugin), bootstrapper);
				}

				x.AddScoped(typeof(IPluginInfoProvider), pluginPackage.InfoProvider)
					.AddScoped(pluginPackage.InfoProvider);
				
				x.AddSingleton(pluginPackage);
			}
		}

		return x;
	}
}