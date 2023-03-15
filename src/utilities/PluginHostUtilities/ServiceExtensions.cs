using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

		services.AddTransient<IPluginResolutionService, PluginResolutionService>();
		services.AddTransient<NuGet.Common.ILogger, NugetLogger>();
		
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
			var logger = pluginProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ServiceExtensions));
			var resolver = pluginProvider.GetRequiredService<IPluginResolutionService>();
        				
			x.Scan(y =>
			{
				var pluginProgress = new Progress<string>(s => progress?.Report(s));
				var resolutionTask = resolver.ResolvePluginsAsync(pluginProgress);
        
				var pluginAssemblies = resolutionTask
					.GetAwaiter()
					.GetResult()
					.Select(a =>
					{
						using (logger.BeginScope(new Dictionary<string, object> {{ "AssemblyPath", a }}))
						{
							try
							{
								logger.LogDebug("Loading assembly @ {AssemblyPath}", a);
								return Assembly.LoadFrom(a);
							}
							catch (FileLoadException ex)
							{
								logger.LogDebug(ex,
									"Failed to load assembly @ {AssemblyPath}, assembly will be excluded from scan",
									a);
								return null;
							}
						}
					})
					.Where(n => n != null)
					.Select(b => b!);
        
        
				y.FromAssemblies(pluginAssemblies)
					.AddClasses(z =>
					{
						z.AssignableTo<IPluginBootstrapper>();
					})
					.AsImplementedInterfaces()
					.WithTransientLifetime()
					.AddClasses(z =>
					{
						z.AssignableTo<IStoragePlugin>();
						z.AssignableTo<IAuthenticatedStoragePlugin>();
					})
					.AsImplementedInterfaces()
					.WithScopedLifetime();
        					
			});
		}

		return x;
	}
}