using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[assembly: ExcludeFromCodeCoverage]

partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution = null!;
    
    [GitRepository]
    readonly GitRepository Repository = null!;

    static readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";
    static readonly AbsolutePath PackagesDirectory = ArtifactsDirectory / "packages";
    static readonly AbsolutePath TestResultsDirectory = ArtifactsDirectory / "test_results";
    static readonly AbsolutePath TraversalProject = RootDirectory / "affected.proj";

    [PublicAPI]
    Target Clean => _ => _
        .Before(ResolveProjects)
        .Before(Restore)
        .Description("Cleans up the output of other build tasks.")
        .Executes(() =>
        {
            // Clean up the solution (except the build project)
            var projs = Solution.GetProjects("*")
                .Where(x => x.Name != "_build");
            DotNetClean(c => c.CombineWith(projs, (_, p) => _.SetProject(p)));
            
            // Clean up the traversal project
            DeleteFile(TraversalProject);
            DeleteDirectory(TraversalProject.Parent / "obj");
            DeleteFile(TraversalProject.Parent / "packages.lock.json");
            GlobFiles(RootDirectory, "**/Generated.Build.g.props").ForEach(DeleteFile);
            
            // Clean up artifacts
            EnsureCleanDirectory(ArtifactsDirectory);
            GlobFiles(RootDirectory, "**/*.*nupkg").ForEach(DeleteFile);
        });

    [PublicAPI]
    Target Restore => _ => _
        .DependsOn(ResolveProjects)
        .Description("Restores NuGet packages for all affected projects.")
        .Executes(() =>
        {
            DotNetRestore(c => c
                .SetProjectFile(TraversalProject)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
        });

    [PublicAPI]
    Target Compile => _ => _
        .After(Version) //Compile doesn't *need* to depend on Version, but it does need to run after it.
        .DependsOn(Restore)
        .Description("Compiles all affected projects.")
        .Executes(() =>
        {
            DotNetBuild(c => c
                .SetProjectFile(TraversalProject)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
        });

    [PublicAPI]
    Target Package => _ => _
        .DependsOn(Compile)
        .DependsOn(Version)
        .Description("Packages affected projects that are configured for upload.")
        .Produces(PackagesDirectory / "*.nupkg", PackagesDirectory / "*.snupkg")
        .Executes(() =>
        {

            DotNetPack(c => c
                .SetProject(TraversalProject)
                .SetOutputDirectory(PackagesDirectory)
                .EnableNoBuild()
            );
        });
}
