using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
	[PackageExecutable("dotnet-stryker", "Stryker.CLI.dll", Framework = "net7.0")]
	readonly Tool Stryker = null!;
	
	[Parameter("The timeout for running tests in minutes.")]
	static readonly int TestTimeout = 8;
	
	[Parameter("The timeout for running mutation tests in minutes.")]
	readonly int MutationTestTimeout = TestTimeout * 2;
	
	[Parameter("The threshold for code coverage.")]
	readonly int CoverageThreshold = IsServerBuild ? 80 : 0;
	
	[Parameter("The threshold for mutation tests.")]
	readonly int MutationThreshold = IsServerBuild ? 100 : 0;
	
	[PublicAPI]
	Target Test => _ => _
		.DependsOn(Compile)
		.Description("Runs automated tests for all affected projects.")
		.Produces(TestResultsDirectory / "**" / "*.xml", TestResultsDirectory / "**" / "*.json")
		.Executes(() =>
		{
			var traversalProject = ProjectModelTasks.ParseProject(TraversalProject);

			var testProjects = traversalProject.GetItems("ProjectReference")
				.Where(x => x.EvaluatedInclude.EndsWith(".Tests.csproj"))
				.Select(x => x.EvaluatedInclude)
				.Select(Solution.GetProject)
				.ToArray();

			DotNetTest(c => c
				.EnableNoBuild()
				.SetBlameHangTimeout($"{TestTimeout}m")
				.EnableCollectCoverage()
				.SetCoverletOutputFormat(CoverletOutputFormat.json)
				.AddLoggers("xunit")
				.SetProperty("Exclude", "[xunit.*]*")
				.SetProperty("SkipAutoProps", "true")
				.SetProperty("DeterministicReport", "true")
				.SetProperty("Threshold", CoverageThreshold)
				.CombineWith(testProjects[..^1], (_, p) => _
					.SetProjectFile(p)
					.SetResultsDirectory(TestResultsDirectory / p.Name)
					.SetCoverletOutput(TestResultsDirectory / "coverage.json")
					.SetProperty("MergeWith", TestResultsDirectory / "coverage.json")
				), 1, //This cannot run in parallel because it would overwrite the coverage file
				true
			);
			
			DotNetTest(c => c
				.EnableNoBuild()
				.SetBlameHangTimeout($"{TestTimeout}m")
				.EnableCollectCoverage()
				.SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
				.AddLoggers("xunit")
				.SetProperty("Exclude", "[xunit.*]*")
				.SetProperty("SkipAutoProps", "true")
				.SetProperty("DeterministicReport", "true")
				.SetProperty("Threshold", CoverageThreshold)
				.SetProjectFile(testProjects[^1])
				.SetResultsDirectory(TestResultsDirectory / testProjects[^1].Name)
				.SetCoverletOutput(TestResultsDirectory / "coverage.xml")
				.SetProperty("MergeWith", TestResultsDirectory / "coverage.json")
			);
			
			foreach (var testProject in testProjects)
			{
				var resultsDirectory = TestResultsDirectory / testProject.Name;
				
				Stryker(
					$"-O {resultsDirectory} -r Json -r Html -r Progress -b {MutationThreshold}",
					testProject.Directory,
					timeout: (int?)TimeSpan.FromMinutes(MutationTestTimeout).TotalMilliseconds
				);
			}
		});
}