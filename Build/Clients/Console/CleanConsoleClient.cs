using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Clients.Console
{
	public class CleanConsoleClient : CleanProject
	{
		protected override string ProjectDirectory => "./src/clients/ConsoleClient/ConsoleClient.csproj";
	}
}