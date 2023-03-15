using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace VespionSoftworks.Athenaeum.Build.Nuget.LocalFeed
{
	public class InitNugetConfigFile : FrostingTask
	{
		/// <inheritdoc />
		public override bool ShouldRun(ICakeContext context)
		{
			return !context.FileExists(context.GetLocalNugetFilePath());
		}

		/// <inheritdoc />
		public override void Run(ICakeContext context)
		{
			context.Information("Creating nuget.config file...");
			var nuget = context.Tools.Resolve("dotnet");
			context.StartProcess(nuget, new ProcessSettings
			{
				Arguments = new ProcessArgumentBuilder()
					.Append("new")
					.Append("nugetconfig")
					.AppendSwitchQuoted("-o", context.GetLocalNugetFeedPath().Combine("..").FullPath)
			});
			base.Run(context);
		}
	}
}