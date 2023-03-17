// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Runtime.InteropServices;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.Help;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using CommandDotNet.NameCasing;
using CommandDotNet.Spectre;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Spectre.Console;
using VespionSoftworks.Athenaeum.Clients.ConsoleClient.Commands;
using VespionSoftworks.Athenaeum.Clients.ConsoleClient.Exceptions;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

//Render header
AnsiConsole.Write(
	new FigletText(FigletFont.Default, "Athenaeum")
		.LeftJustified()
		.Color(Color.Fuchsia)
);
var assem = typeof(Program).Assembly;
var attribs = assem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
var copyright = ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
var assemblyName = assem.GetName();
AnsiConsole.MarkupLine($"[purple]{copyright} - v{assemblyName.Version}[/]");

var runner = new AppRunner<Root>();
IServiceCollection services = new ServiceCollection();

AnsiConsole.Status()
	.AutoRefresh(true)
	.Spinner(Spinner.Known.Dots12)
	.Start("Starting host...", ctx =>
	{
		var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true)
			.AddJsonFile($"appsettings.Development.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

		services.AddSingleton(configuration);
		services.AddLogging(logging =>
		{
			var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

			// IMPORTANT: This needs to be added *before* configuration is loaded, this lets
			// the defaults be overridden by the configuration.
			if (isWindows)
			{
				// Default the EventLogLoggerProvider to warning or above
				logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
			}

			logging.AddConfiguration(configuration.GetSection("Logging"));

			logging.AddDebug();
			logging.AddEventSourceLogger();

			if (isWindows)
			{
				// Add the EventLogLoggerProvider on windows machines
				logging.AddEventLog();
			}

			logging.Configure(options =>
			{
				options.ActivityTrackingOptions =
					ActivityTrackingOptions.SpanId |
					ActivityTrackingOptions.TraceId |
					ActivityTrackingOptions.ParentId;
			});
		});

		services.AddPluginServices(configuration);
		services.AddSingleton(AnsiConsole.Console);

		ctx.Status("Discovering plugins...");
		var pluginProgress = new Progress<string>(s => ctx.Status(s));
		services.ScanForPlugins(pluginProgress);

		ctx.Status("Initializing plugins...");
		services.BootstrapPlugins();

		ctx.Status("Starting host...");
		services.AddSingleton<AppRunner>(runner);

		foreach (var type in runner.GetCommandClassTypes())
		{
			services.AddScoped(type.type);
		}
	});

await using var serviceProvider = services.BuildServiceProvider();

runner
	.UseSpectreAnsiConsole()
	.UseSpectreArgumentPrompter()
	.UseErrorHandler((ctx, ex) =>
	{
		var console = ctx!.DependencyResolver!.Resolve<IAnsiConsole>()!;
		console.Write(ex.GetRenderable());

		if(ex is ISupplyExitCode i)
		{
			return i.ExitCode + 1;
		}

		return 1;
	})
	.UseTypoSuggestions()
	.UseResponseFiles()
	.UseNameCasing(Case.KebabCase)
	.UseMicrosoftDependencyInjection(
		serviceProvider,
		c => c.DependencyResolver!.Resolve<IServiceProvider>()!.CreateScope());

runner.AppSettings.Help.UsageAppNameStyle = UsageAppNameStyle.Adaptive;
runner.AppSettings.Help.TextStyle = HelpTextStyle.Detailed;
runner.AppSettings.Help.ExpandArgumentsInUsage = true;

return await runner.RunAsync(args);