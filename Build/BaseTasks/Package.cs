using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Pack;
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
			context.DotNetPack(ProjectDirectory, new DotNetPackSettings
			{
				NoBuild = true,
				NoRestore = true,
				IncludeSymbols = false,
				Configuration = context.BuildConfiguration,
				Verbosity = DotNetVerbosity.Normal,
				OutputDirectory = context.Environment.WorkingDirectory
					.Combine("artifacts").Combine(ArtifactSubDirectory)
			});
		}
	}
}