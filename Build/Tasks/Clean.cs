using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.Plugins.Abstractions;
using VespionSoftworks.Athenaeum.Build.Plugins.Storage.Abstractions;
using VespionSoftworks.Athenaeum.Build.Plugins.Storage.Filesystem;

namespace VespionSoftworks.Athenaeum.Build.Tasks
{
	[IsDependentOn(typeof(CleanFileSystemPlugin))]
	[IsDependentOn(typeof(CleanStoragePluginAbstractions))]
	[IsDependentOn(typeof(CleanPluginAbstractions))]
	public class Clean: FrostingTask<BuildContext>
	{
		
	}
}