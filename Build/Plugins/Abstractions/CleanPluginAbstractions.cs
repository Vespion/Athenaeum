using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Abstractions
{
	public class CleanPluginAbstractions: CleanProjectAndFeed
	{
		protected override string ProjectFile => "./src/plugins/PluginAbstractions/PluginAbstractions.csproj";
		protected override string ArtifactSubDirectory => "plugins/storage/abstractions";
	}
}