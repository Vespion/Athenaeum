using System.Linq;
using Cake.Common.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.NuGet.Delete;
using Cake.Core.IO;
using Cake.Frosting;

namespace VespionSoftworks.Athenaeum.Build.BaseTasks
{
	public abstract class CleanProject: FrostingTask<BuildContext>
	{
		protected abstract string ProjectDirectory { get; }
		
		/// <inheritdoc />
		public override void Run(BuildContext context)
		{
			context.DotNetCoreClean(ProjectDirectory);
		}
	}
	
	public abstract class CleanProjectAndFeed: CleanProject
	{
		protected abstract string ArtifactSubDirectory { get; }
		
		/// <inheritdoc />
		public override void Run(BuildContext context)
		{
			base.Run(context);
			
			var artifacts = context.Environment.WorkingDirectory
				.Combine("artifacts").Combine(ArtifactSubDirectory);

			var pkgs = context.Globber.GetFiles(artifacts.Combine("*.nupkg").FullPath);
			foreach (var filePath in pkgs)
			{
				var name = filePath.GetFilenameWithoutExtension().ToString().Split('.');

				var pkgName = string.Join('.', name.SkipLast(3));
				var pkgVersion = string.Join('.', name.TakeLast(3));
				
				context.DotNetCoreNuGetDelete(pkgName, pkgVersion, new DotNetCoreNuGetDeleteSettings
				{
					Source = context.PluginFeed,
					ApiKey = context.PluginFeedKey,
					Interactive = false,
					NonInteractive = true
				});
				context.DeleteFile(filePath);
			}
		}
	}
}