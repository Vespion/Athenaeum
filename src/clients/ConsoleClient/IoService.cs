using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace VespionSoftworks.Athenaeum.Clients.ConsoleClient;

public class IoService: BackgroundService
{
	private bool _isRunning = true;
	private readonly IAnsiConsole _console;
	private readonly IHostApplicationLifetime _appLifetime;
	private readonly ILogger<IoService> _logger;
	private readonly IConfiguration _configuration;

	/// <inheritdoc />
	public IoService(IAnsiConsole console, IHostApplicationLifetime appLifetime, ILogger<IoService> logger, IConfiguration configuration)
	{
		_console = console;
		_appLifetime = appLifetime;
		_logger = logger;
		_configuration = configuration;
	}

	/// <inheritdoc />
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		
		void EvaluateCommand(string args)
		{
			if (args == "exit")
			{
				_isRunning = false;
			}
			else if (args == "test_1")
			{
				_logger.LogCritical("Success 1!");
			}
			else if (args == "test_2")
			{
				_logger.LogCritical("Success 2!");
			}
		}
		
		var suppliedCommands = _configuration.GetSection("commands").Get<string[]>();

		if (suppliedCommands?.Length > 0)
		{
			_logger.LogInformation("Host launched, running supplied commands");
			
			foreach (var command in suppliedCommands)
			{
				_logger.LogDebug("Running command: {Command}", command);
				EvaluateCommand(command);
			}
		}
		else
		{
			_logger.LogInformation("Host launched, starting execution loop");
			_console.WriteLine("Welcome to the Athenaeum Console Client!");
			_console.WriteLine("Type 'help' for a command list.", new Style(decoration: Decoration.Italic));
			_console.WriteLine();
		
			while (_isRunning)
			{
				var input = _console.Ask<string>("> ");
				_logger.LogDebug("Received input: {Input}", input);
				EvaluateCommand(input);
			}
		}

		_logger.LogInformation("Execution loop finished, triggering host shutdown");
		_appLifetime.StopApplication();
		return Task.CompletedTask;
	}
}