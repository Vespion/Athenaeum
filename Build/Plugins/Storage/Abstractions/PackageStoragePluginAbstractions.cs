using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Storage.Abstractions
{
	[IsDependentOn(typeof(BuildStoragePluginAbstractions))]
	public class PackageStoragePluginAbstractions: Package
	{
		protected override string ProjectDirectory => "./src/plugins/storage/StoragePluginAbstractions/StoragePluginAbstractions.csproj";
		protected override string ArtifactSubDirectory => "plugins/storage/abstractions";
	}
}