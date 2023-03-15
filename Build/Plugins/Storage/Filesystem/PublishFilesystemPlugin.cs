using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Storage.Filesystem
{
	[IsDependentOn(typeof(PackageFilesystemPlugin))]
	public class PublishFilesystemPlugin: PublishToFeed
	{
		protected override string ArtifactSubDirectory => "plugins/storage/filesystem";
	}
}