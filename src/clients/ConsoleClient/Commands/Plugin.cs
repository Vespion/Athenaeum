using CommandDotNet;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

namespace VespionSoftworks.Athenaeum.Clients.ConsoleClient.Commands;

public class Plugin
{
	[PublicAPI]
	[Command(Description = "Lists installed plugins")]
	public void List(
		IAnsiConsole console,
		IEnumerable<PluginPackage> packages,
		IPluginPackageAccessor packageAccessor,
		ILogger<Plugin> logger,
		[Option('j', "json", Description = "Output results as JSON")]bool json = false
	)
	{
		void WriteJson()
		{
			
		}
		
		void WriteTable()
		{
			var table = new Table();
			console.Live(table)
				.Start(ctx =>
				{
					table.AddColumns("Name", "Version", "Description", "Author");
					ctx.Refresh();
					
					foreach (var package in packages)
					{
						var provider = packageAccessor.GetInfoProviderForType(package.InfoProvider);
						
						table.AddRow(
							new Markup(provider.Name),
							new Markup(package.Header.Version.ToString()),
							provider.Description != null
								? new Markup(provider.Description)
								: new Markup("[red]No description provided[/]"),
							new Markup(
								provider.Author
							)
						);
						ctx.Refresh();
					}
				});
		}
		
		if (json)
		{
			WriteJson();
		}
		else
		{
			WriteTable();
		}
	}
}