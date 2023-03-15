using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Storage.Abstractions
{
	[IsDependentOn(typeof(PackageStoragePluginAbstractions))]
	public class PublishStoragePluginAbstractions: PublishToFeed
	{
		protected override string ArtifactSubDirectory => "plugins/storage/abstractions";
	}
}