using System;
using System.IO;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.PathConstruction;

partial class Build
{
	[PublicAPI]
	Target Publish => _ => _
		.DependsOn(Test)
		.DependsOn(Package)
		.Description("Publishes changed projects")
		.Executes(() =>
		{
			if (PublishNugetPackages)
			{
				Log.Debug("Publishing NuGet packages");
				PublishToNugetFeed();
			}
			else
			{
				Log.Debug("Skipping NuGet package publishing");
			}
		});
	
	[Parameter("Publish NuGet packages")]
	readonly bool PublishNugetPackages = IsServerBuild;
	
	[Parameter("NuGet API Key for publishing packages")]
	[Secret]
	readonly string? NuGetApiKey;
    
	[Parameter("NuGet Symbols API Key for publishing packages")]
	[Secret]
	readonly string? NuGetSymbolsApiKey;
    
	[Parameter("NuGet feed URL for publishing packages")]
	readonly string NugetFeed = PackagesDirectory / ".feed";
    
	[Parameter("NuGet symbols feed URL for publishing packages")]
	readonly string? NugetSymbolsFeed;
	
	void PublishToNugetFeed()
	{
		var packages = GlobFiles(PackagesDirectory, "*.nupkg");

		if (packages.Count <= 0)
		{
			Log.Information("No packages in artifact directory, skipping nuget publish");
			return;
		}
		
		Log.Information("Found {Count} packages to publish", packages.Count);

		Log.Debug("Publishing to {NugetFeed}", NugetFeed);

		if (!string.IsNullOrWhiteSpace(NugetSymbolsFeed))
		{
			Log.Verbose("Publishing symbols to {NugetSymbolsFeed}", NugetSymbolsFeed);
		}
            
		if (!string.IsNullOrWhiteSpace(NuGetApiKey))
		{
			Log.Verbose("Publishing with API key");
		}
		if (!string.IsNullOrWhiteSpace(NuGetSymbolsApiKey))
		{
			Log.Verbose("Publishing symbols with API key");
		}

		//If the feed is a local directory, create it
		try
		{
			var full = Path.GetFullPath(NugetFeed);
			Directory.CreateDirectory(full);
		}
		catch
		{
			//This is fine, it's not a local directory so we just hand it nuget to deal with
		}
		
		DotNetNuGetPush(c => c
				.SetSource(NugetFeed)
				.EnableSkipDuplicate()
				.SetSymbolSource(NugetSymbolsFeed)
				.SetApiKey(NuGetApiKey)
				.SetSymbolApiKey(NuGetSymbolsApiKey)	
				.CombineWith(
					packages, (_, v) => _
						.SetTargetPath(v)
				),
			5,
			true);

		Log.Information("NuGet publishing complete");
	}
}