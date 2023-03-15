using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Frosting;

namespace VespionSoftworks.Athenaeum.Build.BaseTasks
{
	public abstract class Build: FrostingTask<BuildContext>
	{
		protected abstract string ProjectDirectory { get; }
		
		/// <inheritdoc />
		public override void Run(BuildContext context)
		{
			context.DotNetCoreBuild(ProjectDirectory, new DotNetCoreBuildSettings
			{
				Configuration = context.BuildConfiguration,
				NoRestore = false,
				NoDependencies = true,
				NoIncremental = false,
				NoLogo = true,
				Verbosity = DotNetCoreVerbosity.Normal
			});
		}
	}
}