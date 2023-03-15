using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Pack;
using Cake.Frosting;

namespace VespionSoftworks.Athenaeum.Build.BaseTasks
{
	public abstract class Package: FrostingTask<BuildContext>
	{
		protected abstract string ProjectDirectory { get; }
		protected abstract string ArtifactSubDirectory { get; }
		
		/// <inheritdoc />
		public override void Run(BuildContext context)
		{
			context.DotNetCorePack(ProjectDirectory, new DotNetCorePackSettings
			{
				NoBuild = true,
				NoRestore = true,
				IncludeSymbols = false,
				Configuration = context.BuildConfiguration,
				Verbosity = DotNetCoreVerbosity.Normal,
				OutputDirectory = context.Environment.WorkingDirectory
					.Combine("artifacts").Combine(ArtifactSubDirectory)
			});
		}
	}
}