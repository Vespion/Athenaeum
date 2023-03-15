using System.IO;
using System.Linq;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.NuGet.Push;
using Cake.Frosting;

namespace VespionSoftworks.Athenaeum.Build.BaseTasks
{
	public abstract class PublishToFeed: FrostingTask<BuildContext>
	{
		protected abstract string ArtifactSubDirectory { get; }
		
		/// <inheritdoc />
		public override void Run(BuildContext context)
		{

			var pkgDirectory = context.Environment.WorkingDirectory
				.Combine("artifacts").Combine(ArtifactSubDirectory);

			var pkgFiles = context.GetFiles(pkgDirectory.Combine("*.nupkg").FullPath);

			var pkg = pkgFiles
				.Select(x => new FileInfo(x.FullPath))
				.OrderByDescending(x => x.LastWriteTime)
				.First();
			
			context.DotNetNuGetPush(pkg.FullName, new DotNetNuGetPushSettings
			{
				Source = context.PluginFeed,
				ApiKey = context.PluginFeedKey
			});
		}
	}
}