using System.Diagnostics.CodeAnalysis;
using CommandDotNet;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using VespionSoftworks.Athenaeum.Clients.ConsoleClient.Exceptions;

namespace VespionSoftworks.Athenaeum.Clients.ConsoleClient.Commands;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "CommandDotNet requires non-static methods")]
public class Root
{
	private static bool RunInteractive { get; set; }
	
	// [Command(Description = "Shows help information")]
	// public void Help(CommandContext context)
	// {
	// 	context.PrintHelp();
	// }
	
	[PublicAPI]
	[Command(Description = "Starts interactive mode")]
	public void Interactive(AppRunner runner, IAnsiConsole console, ILogger<Root> logger)
	{
		if (RunInteractive)
		{
			throw new NestedInteractiveLoopDetectedException();
		}
		
		RunInteractive = true;
		while (RunInteractive)
		{
			var input = console.Ask<string>("> ");
			logger.LogDebug("Received input: {Input}", input);
			var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			runner.Run(args);
		}
	}
	
	[PublicAPI]
	[Command(DescriptionLines = new []
	{
		"Exits interactive mode.",
		"Note: This command is only available in interactive mode, if run outside of interactive mode it is a no-op."
	})]
	public void Exit()
	{
		RunInteractive = false;
	}
}