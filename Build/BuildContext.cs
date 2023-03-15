using Cake.Core;
using Cake.Frosting;
using VespionSoftworks.Athenaeum.Build.Nuget;

namespace VespionSoftworks.Athenaeum.Build
{
	public class BuildContext: FrostingContext
	{
		public BuildContext(ICakeContext context) : base(context)
		{
		}

		public string BuildConfiguration => Arguments.GetArgument("configuration");

		public string PluginFeed => Arguments.GetArgument("plugin-feed") ?? this.GetLocalNugetFeedPath().FullPath;
		public string PluginFeedKey => Arguments.GetArgument("plugin-feed-api-kay");
	}
}