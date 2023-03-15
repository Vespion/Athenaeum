using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.BaseTasks;

namespace VespionSoftworks.Athenaeum.Build.Plugins.Abstractions
{
	[IsDependentOn(typeof(BuildPluginAbstractions))]
	public class PackagePluginAbstractions: Package
	{
		protected override string ProjectDirectory => "./src/plugins/PluginAbstractions/PluginAbstractions.csproj";
		protected override string ArtifactSubDirectory => "plugins/storage/abstractions";
	}
}