using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.Plugins.Abstractions;
using VespionSoftworks.Athenaeum.Build.Plugins.Storage.Abstractions;
using VespionSoftworks.Athenaeum.Build.Plugins.Storage.Filesystem;

namespace VespionSoftworks.Athenaeum.Build.Tasks.Plugins
{
	[IsDependentOn(typeof(PublishFilesystemPlugin))]
	[IsDependentOn(typeof(PublishStoragePluginAbstractions))]
	[IsDependentOn(typeof(PublishPluginAbstractions))]
	public class PublishAllStoragePlugins: FrostingTask
	{
		
	}
}