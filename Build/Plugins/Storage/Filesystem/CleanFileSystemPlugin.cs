using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Storage.Filesystem
{
	public class CleanFileSystemPlugin: CleanProjectAndFeed
	{
		protected override string ProjectFile => "./src/plugins/storage/FilesystemStorage/FilesystemStorage.csproj";
		protected override string ArtifactSubDirectory => "plugins/storage/filesystem";
	}
}