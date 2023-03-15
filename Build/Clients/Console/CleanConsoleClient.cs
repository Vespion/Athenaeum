using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Clients.Console
{
	public class CleanConsoleClient : CleanProjectAndPlugins
	{
		protected override string ProjectFile => "./src/clients/ConsoleClient/ConsoleClient.csproj";
	}
}