using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Storage.Filesystem
{
	[IsDependentOn(typeof(BuildFilesystemPlugin))]
	public class PackageFilesystemPlugin: Package
	{
		protected override string ProjectDirectory => "./src/plugins/storage/FilesystemStorage/FilesystemStorage.csproj";
		protected override string ArtifactSubDirectory => "plugins/storage/filesystem";
	}
}