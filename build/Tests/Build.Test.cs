using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Octokit;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.TextTasks;

// ReSharper disable CheckNamespace
partial class Build
{
	[PackageExecutable("dotnet-stryker", "Stryker.CLI.dll", Framework = "net7.0")]
	readonly Tool Stryker = null!;
	
	[Parameter("The timeout for running tests in minutes.")]
	static readonly int TestTimeout = 8;
	
	[Parameter("The timeout for running mutation tests in minutes.")]
	readonly int MutationTestTimeout = TestTimeout * 2;
	
	//When running on the server, we don't want to fail the build if the coverage is below the threshold.
	//This is because the server build will generate a check run which will block the commit if it fails.
	[Parameter("The threshold for code coverage.")]
	readonly int CoverageThreshold = IsServerBuild ? 0 : 80;
	
	[Parameter("The threshold for mutation tests.")]
	readonly int MutationThreshold = IsServerBuild ? 0 : 100;
	
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
					$"-O {resultsDirectory} -r Json -r Html -r Progress -b {MutationThreshold} --target-framework net7.0",
					testProject.Directory,
					timeout: (int?)TimeSpan.FromMinutes(MutationTestTimeout).TotalMilliseconds
				);
			}
		});

	[PublicAPI]
	Target PublishMutationTestResults => _ => _
		.Requires(() => IsServerBuild)
		.Description("Publishes the mutation test results as a check run.")
		.TriggeredBy(Test)
		.ProceedAfterFailure()
		.Unlisted()
		.Executes(async () =>
		{
			var repositoryName = GitHubActions.Repository.Split('/')[1];
			var repositoryOwnerName = GitHubActions.Repository.Split('/')[0];
			
			var reports = GlobFiles(TestResultsDirectory, "**/reports/mutation-report.json");

			var client = new ChecksClient(GetGithubApiConnection());

			var newCheck = new NewCheckRun("mutation-tests", GitHubActions.Sha)
			{
				Status = CheckStatus.InProgress,
				StartedAt = DateTimeOffset.Now
			};
			
			var check = await client.Run.Create(repositoryOwnerName, repositoryName, newCheck);
			
			var annotations = new List<NewCheckRunAnnotation>();

			var totalMutantsTimedOut = 0;
			var totalMutantsKilled = 0;
			var totalMutantsSurvived = 0;
			var totalMutantsNoCoverage = 0;

			var mutationTable = new StringBuilder()
				.AppendLine("| File  | Killed  | **Survived**  | Timed Out  | **No Coverage**  | Total  |")
				.AppendLine("| ------------ | ------------ | ------------ | ------------ | ------------ | ------------ |");
			
			foreach (var report in reports)
			{
				var fileContents = ReadAllText(report);
				var mutationReport = Serialize.FromJson(fileContents);
				
				foreach (var (key, value) in mutationReport!.Files)
				{
					var filePath = Solution.Directory.GetRelativePathTo(key);

					var mutantsTimedOut = 0;
					var mutantsKilled = 0;
					var mutantsSurvived = 0;
					var mutantsNoCoverage = 0;
					
					foreach (var mutant in value.Mutants)
					{
						CheckAnnotationLevel level;
						switch (mutant.Status)
						{
							case MutantStatus.Killed:
								mutantsKilled++;
								level = CheckAnnotationLevel.Notice;
								break;
							case MutantStatus.CompileError:
							case MutantStatus.Ignored:
								level = CheckAnnotationLevel.Notice;
								break;
							case MutantStatus.NoCoverage:
								level = CheckAnnotationLevel.Failure;
								mutantsNoCoverage++;
								break;
							case MutantStatus.RuntimeError:
								level = CheckAnnotationLevel.Notice;
								break;
							case MutantStatus.Survived:
								level = CheckAnnotationLevel.Failure;
								mutantsSurvived++;
								break;
							case MutantStatus.Timeout:
								level = CheckAnnotationLevel.Notice;
								mutantsTimedOut++;
								break;
							default:
								level = CheckAnnotationLevel.Failure;
								break;
						}

						//Short and snappy
						var title = mutant.MutatorName;
						
						var description = new StringBuilder();

						if (!string.IsNullOrWhiteSpace(mutant.Description))
						{
							description.AppendLine(mutant.Description)
								.AppendLine();
						}

						description.Append("Status: ").AppendLine(mutant.Status.Humanize());
						description.Append("Due to: ").AppendLine(mutant.StatusReason);
						description.AppendLine();
						if (!string.IsNullOrWhiteSpace(mutant.Replacement))
						{
							description.AppendLine();
							description.AppendLine("Replacement: ").AppendLine(mutant.Replacement);
						}

						var annotation = new NewCheckRunAnnotation(
							filePath,
							(int)mutant.Location.Start.Line,
							(int)mutant.Location.End.Line,
							level,
							description.ToString()
						)
						{
							StartColumn = mutant.Location.Start.Line == mutant.Location.End.Line ? (int)mutant.Location.Start.Column : null,
							EndColumn = mutant.Location.Start.Line == mutant.Location.End.Line ? (int)mutant.Location.End.Column : null,
							RawDetails = mutant.ToJson(),
							Title = title
						};
						
						annotations.Add(annotation);
					}
					
					totalMutantsTimedOut += mutantsTimedOut;
					totalMutantsKilled += mutantsKilled;
					totalMutantsSurvived += mutantsSurvived;
					totalMutantsNoCoverage += mutantsNoCoverage;
					
					var mutantsDetected = mutantsTimedOut + mutantsKilled;
					var mutantsUndetected = mutantsSurvived + mutantsNoCoverage;
					var mutants = mutantsDetected + mutantsUndetected;
					
					mutationTable
						.Append("| ").Append(filePath).Append(" | ").Append(mutantsKilled).Append(" | ")
						.Append(mutantsSurvived).Append(" | ").Append(mutantsTimedOut).Append(" | ")
						.Append(mutantsNoCoverage).Append(" | ").Append(mutants).Append(" |");
				}
			}
			
			var totalMutantsDetected = totalMutantsTimedOut + totalMutantsKilled;
			var totalMutantsUndetected = totalMutantsSurvived + totalMutantsNoCoverage;
			var totalMutants = totalMutantsDetected + totalMutantsUndetected;
			// ReSharper disable once IntDivisionByZero
			var mutationScore = totalMutantsDetected / totalMutants * 100;
			
			var checkConclusion = mutationScore >= MutationThreshold ? CheckConclusion.Success : CheckConclusion.Failure;

			var annotationChunks = annotations.Chunk(45).ToArray();
			
			var checkSummaryPassStatus = checkConclusion == CheckConclusion.Success ? ":white_check_mark: Mutation coverage threshold passed!" : ":x: Mutation coverage threshold missed!";
			
			var checkSummary = $@"# Mutation Tests

------------

## Mutation Score: *{mutationScore}*%
### Threshold: {MutationThreshold}%

{checkSummaryPassStatus}

{mutationTable}

Results for commit {GitHubActions.Sha}";

			CheckRunUpdate? update;
			foreach (var checkRunAnnotations in annotationChunks[..^1])
			{
				update = new CheckRunUpdate
				{
					Output = new NewCheckRunOutput("Mutation Test Results", checkSummary)
					{
						Annotations = checkRunAnnotations
					}
				};

				check = await client.Run.Update(repositoryOwnerName, repositoryName, check.Id, update);
			}

			update = new CheckRunUpdate
			{
				Conclusion = checkConclusion,
				CompletedAt = DateTimeOffset.Now,
				Status = CheckStatus.Completed,
				Output = new NewCheckRunOutput("Mutation Test Results", checkSummary)
				{
					Annotations = annotationChunks[^1]
				}
			};
			
			await client.Run.Update(repositoryOwnerName, repositoryName, check.Id, update);
			
			Log.Debug("All run annotations have been posted to GitHub");
			
			var commitClient = new CommitStatusClient(GetGithubApiConnection());
			
			await commitClient.Create(repositoryOwnerName, repositoryName, GitHubActions.Sha, new NewCommitStatus
			{
				Context = "mutation-tests",
				Description = checkSummaryPassStatus,
				State = checkConclusion == CheckConclusion.Success ? CommitState.Success : CommitState.Failure
			});
			
			Log.Debug("Commit status updated");
		});
}