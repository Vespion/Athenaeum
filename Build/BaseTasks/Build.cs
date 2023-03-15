using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Frosting;

namespace VespionSoftworks.Athenaeum.Build.BaseTasks
{
	public abstract class Build: FrostingTask<BuildContext>
	{
		protected abstract string ProjectDirectory { get; }
		
		/// <inheritdoc />
		public override void Run(BuildContext context)
		{
			context.DotNetBuild(ProjectDirectory, new DotNetBuildSettings
			{
				Configuration = context.BuildConfiguration,
				NoRestore = false,
				NoDependencies = true,
				NoIncremental = false,
				NoLogo = true,
				Verbosity = DotNetVerbosity.Normal
			});
		}
	}
}