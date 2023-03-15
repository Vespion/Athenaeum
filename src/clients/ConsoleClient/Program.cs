// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using VespionSoftworks.Athenaeum.Clients.ConsoleClient;
using VespionSoftworks.Athenaeum.Plugins.Abstractions;
using VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

//Render header
AnsiConsole.Write(
	new FigletText("Athenaeum")
		.LeftJustified()
		.Color(Color.Fuchsia)
);
var assem = typeof(Program).Assembly;
var attribs = assem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
var copyright = ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
var assemblyName = assem.GetName();
AnsiConsole.MarkupLine($"[grey]{copyright} - v{assemblyName.Version}[/]");

//Build generic host
IHost host = null!;
AnsiConsole.Status()
	.AutoRefresh(true)
	.Spinner(Spinner.Known.Dots12)
	.Start("Starting host...", ctx =>
	{
		//Configure logging and shared services
		var builder = Host.CreateDefaultBuilder(args)
			.UseConsoleLifetime()
			.ConfigureLogging(lb =>
			{
				lb.ClearProviders();

				lb.AddDebug();
				lb.AddEventSourceLogger();
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					lb.AddEventLog();
				}
			})
			.ConfigureServices((context, services) =>
			{
				services.AddPluginServices(context);
				services.AddSingleton(AnsiConsole.Console);
			});
		
		//Resolve and bootstrap plugins
		builder.ConfigureServices(x =>
		{
			ctx.Status("Discovering plugins...");
			using (var pluginProvider = x.BuildServiceProvider())
			{
				var logger = pluginProvider.GetRequiredService<ILogger<Program>>();
				var resolver = pluginProvider.GetRequiredService<IPluginResolutionService>();
				
				x.Scan(y =>
				{
					var pluginProgress = new Progress<string>(s => ctx.Status(s));
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

			ctx.Status("Initializing plugins...");
			using (var bootstrapProvider = x.BuildServiceProvider())
			{
				var bootstrappers = bootstrapProvider.GetServices<IPluginBootstrapper>();
				foreach (var bootstrapper in bootstrappers)
				{
					bootstrapper.ConfigureServices(x);
				}
			}
			
			ctx.Status("Starting host...")
				.Spinner(Spinner.Known.Dots12);
		});

		builder.ConfigureServices(x =>
		{
			x.AddHostedService<IoService>();
		});
		
		host = builder.Build();
	});

await host.RunAsync();
