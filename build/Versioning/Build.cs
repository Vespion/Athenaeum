using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Semver;
using Serilog;
// ReSharper disable CheckNamespace

partial class Build
{
    /// <summary>
    /// The cache of commits for paths. Don't access this directly, use <see cref="ScanForCommits"/> which will manage the cache.
    /// </summary>
    /// <remarks>
    /// Construction of the commit history is expensive, so we cache it.
    /// We use a <see cref="Lazy{T}"/> to ensure that the search delegate is only called once even if the dictionary
    /// factory method is called multiple times.
    /// See: https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
    /// </remarks>
    static readonly ConcurrentDictionary<RelativePath, Lazy<IEnumerable<ConventionalCommit>>> CommitsCache = new();

    static IEnumerable<ConventionalCommit> ScanForCommits(RelativePath target, GitRepository repository)
    {
        Log.Debug("Checking cache for commits targeting {Path}", target);

        return CommitsCache.GetOrAdd(target, t =>
        {
            Log.Verbose("Cache miss for {Path}, building lazy delegate", t); 
            return new Lazy<IEnumerable<ConventionalCommit>>(() =>
            {
                Log.Debug("Cache miss for {Path}, scanning for commits", t);

                var parser = new ConventionalCommitParser();
                
                using var repo = new Repository(repository.LocalDirectory);
                
                Log.Verbose("Repository path: {Path}", repo.Info.WorkingDirectory);
                
                var filter = new CommitFilter
                {
                    SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time,
                    IncludeReachableFrom = repo.Head
                };
                
                Log.Verbose("Commit filter: {@Filter}", new Dictionary<string, object?>
                {
                    {"SortBy", filter.SortBy},
                    {"IncludeReachableFrom", (filter.IncludeReachableFrom as Branch)?.CanonicalName}
                });
                
                var logEntries = repo.Commits.QueryBy(t, filter)
                    .ToList();
                logEntries.Reverse();
                Log.Debug("Found {Count} commits, running semantic analysis", logEntries.Count);
                
                var commits = new List<ConventionalCommit>(logEntries.Count);
                // Using LINQ here causes a stack overflow...I think, either way it crashes out with no error message or anything.
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (var logEntry in logEntries)
                {
                    commits.Add(parser.Parse(logEntry.Commit));
                }

                return commits;
            });
        }).Value;
    }
    
	Target Version => _ => _
        .DependsOn(ResolveProjects)
        .Description("Calculates semantic versions for projects")
        .Unlisted()
        .Executes(() =>
        {
            const string itemType = "ProjectReference";
            
            Log.Information("Calculating semantic versions for projects...");
            
            var traversalProject = ProjectModelTasks.ParseProject(TraversalProject);
            var projectRefs = traversalProject.GetItems(itemType)
                .ToArray();
            
            Log.Verbose("Found {Count} project references", projectRefs.Length);
            
            foreach (var projectRef in projectRefs)
            {
                Log.Verbose("Resolving {Project} from solution", projectRef.EvaluatedInclude);
                var proj = Solution.GetProject(projectRef.EvaluatedInclude)!;

                Log.Debug("Calculating semantic version for {Project}", proj.Name);
                
                var projRelativePath = Solution.Directory.GetRelativePathTo(proj.Directory);
                
                Log.Verbose("Resolved relative path {Path}", projRelativePath);
                
                var commits = ScanForCommits(projRelativePath, Repository).ToArray();
                
                Log.Debug("Found {Count} commits", commits.Length);
                var version = new SemVersion(1);

                if (!Repository.IsOnMainBranch())
                {
                    var branch = new PrereleaseIdentifier(Repository.Branch?.Replace('/', '-') ?? "detached");
                    var commitCount = new PrereleaseIdentifier(commits.Length);
                    
                    version = version.WithPrerelease(new []
                    {
                        branch,
                        commitCount
                    });
                }

                var commitHash = Repository.Commit;
                foreach (var commit in commits)
                {
                    if (commit.IsBreaking)
                    {
                        version = new SemVersion(version.Major + 1, 0, 0, version.PrereleaseIdentifiers,
                            version.MetadataIdentifiers);
                        continue;
                    }

                    switch (commit.Type.ToLower())
                    {
                        case "docs":
                        case "style":
                        case "refactor":
                        case "perf":
                        case "test":
                        case "build":
                        case "ci":
                        case "chore":
                            break;
                        case "fix":
                            version = new SemVersion(version.Major, version.Minor, version.Patch + 1, version.PrereleaseIdentifiers,
                                version.MetadataIdentifiers);
                            break;
                        default:
                            version = new SemVersion(version.Major, version.Minor + 1, 0, version.PrereleaseIdentifiers,
                                version.MetadataIdentifiers);
                            break;
                    }

                    commitHash = commit.Sha;
                }

                version = version.WithMetadataParsedFrom(commitHash);
                
                Log.Information("{Project} -> {Version}", proj.Name, version);

                UpdateBuildProperty(proj.Directory, "VersionPrefix", $"{version.Major}.{version.Minor}.{version.Patch}");
                UpdateBuildProperty(proj.Directory, "VersionSuffix", $"{version.Prerelease}+{version.Metadata}");
                
                Log.Debug("Calculated version {Version} for project {Project}", version, proj.Name);
            }
        });
}