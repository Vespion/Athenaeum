using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Storage.Abstractions
{
	public class CleanStoragePluginAbstractions: CleanProjectAndFeed
	{
		protected override string ProjectDirectory => "./src/plugins/storage/StoragePluginAbstractions/StoragePluginAbstractions.csproj";
		protected override string ArtifactSubDirectory => "plugins/storage/abstractions";
	}
}