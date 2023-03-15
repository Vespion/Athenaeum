using Cake.Common.Tools.NuGet;
using Cake.Common.Tools.NuGet.Sources;
using Cake.Core;
using Cake.Frosting;

namespace VespionSoftworks.Athenaeum.Build.Nuget.LocalFeed
{
	[IsDependentOn(typeof(InitNugetConfigFile))]
	public class CreateLocalFeed : FrostingTask
	{
		/// <inheritdoc />
		public override bool ShouldRun(ICakeContext context)
		{
			return !context.NuGetHasSource("Local Solution", new NuGetSourcesSettings
			{
				ConfigFile = context.GetLocalNugetFilePath()
			});
		}

		/// <inheritdoc />
		public override void Run(ICakeContext context)
		{
			context.NuGetAddSource("Local Solution", context.GetLocalNugetFeedPath().FullPath, new NuGetSourcesSettings
			{
				ConfigFile = context.GetLocalNugetFilePath()
			});
		}
	}
}