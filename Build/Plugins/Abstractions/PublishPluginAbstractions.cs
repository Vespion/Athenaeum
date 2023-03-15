using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Abstractions
{
	[IsDependentOn(typeof(PackagePluginAbstractions))]
	public class PublishPluginAbstractions: PublishToFeed
	{
		protected override string ArtifactSubDirectory => "plugins/storage/abstractions";
	}
}