using Cake.Core;
using Cake.Core.IO;

namespace VespionSoftworks.Athenaeum.Build.Nuget
{
	public static class NugetHelpers
	{
		public static FilePath GetLocalNugetFilePath(this ICakeContext context)
		{
			var nugetCachePath = context.Environment.WorkingDirectory.Combine(".cache/nuget");
			return nugetCachePath.GetFilePath("nuget.config");
		}

		public static DirectoryPath GetLocalNugetFeedPath(this ICakeContext context)
		{
			var nugetCachePath = context.Environment.WorkingDirectory.Combine(".cache/nuget");
			var feedPath = nugetCachePath.Combine("feed");
			context.FileSystem.GetDirectory(feedPath).Create();
			return feedPath;
		}
	}
}