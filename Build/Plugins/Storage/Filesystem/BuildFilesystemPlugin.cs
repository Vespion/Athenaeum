using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.Plugins.Abstractions;
using VespionSoftworks.Athenaeum.Build.Plugins.Storage.Abstractions;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Storage.Filesystem
{
	[IsDependentOn(typeof(BuildPluginAbstractions))]
	[IsDependentOn(typeof(BuildStoragePluginAbstractions))]
	public class BuildFilesystemPlugin: BaseTasks.Build
	{
		protected override string ProjectDirectory => "./src/plugins/storage/FilesystemStorage/FilesystemStorage.csproj";
	}
}