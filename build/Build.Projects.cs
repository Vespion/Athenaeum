using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Serilog;

partial class Build
{
    [Parameter("List of projects to build")]
    readonly string[]? TargetProject;
    
    [Parameter("Build all projects")]
    readonly bool BuildAll;
    
    [Parameter("Build changed projects")]
    readonly bool BuildChanged;
    
    [PackageExecutable(
        packageId: "dotnet-affected",
        packageExecutable: "dotnet-affected.dll",
        Framework = "net7.0")]
    readonly Tool DotnetAffected = null!;
    
	Target ResolveProjects => _ => _
        .Unlisted()
        .Executes(() =>
        {
            const string traversalSdk = "Microsoft.Build.Traversal/3.0.3";
            if (BuildAll)
            {
                if (TargetProject?.Length > 0)
                {
                    Log.Warning("All projects were specified along with other projects, ignoring other projects");
                }

                Log.Information("All projects selected for build");
                
                var proj = new Microsoft.Build.Evaluation.Project
                {
                    Xml =
                    {
                        Sdk = traversalSdk
                    }
                };

                foreach (var project in Solution.GetProjects("*"))
                {
                    if(project.Name.EndsWith("_build"))
                        continue;
                    
                    proj.AddItemFast("ProjectReference", project.Path);
                }
                
                proj.Save(TraversalProject);
                
                Log.Debug("Traversal project saved to {TraversalProject}", TraversalProject);

                ProjectCollection.GlobalProjectCollection.TryUnloadProject(proj.Xml);
            }
            else if (BuildChanged)
            {
                if (TargetProject?.Length > 0)
                {
                    
                }
                
                Log.Information("Changed projects selected for build");
                
                DotnetAffected($"--solution-path {Solution.Path} -v -f traversal --output-dir {RootDirectory}");
                
                var traversalProject = ProjectModelTasks.ParseProject(TraversalProject);
                var buildProj = traversalProject.GetItems("ProjectReference")
                    .FirstOrDefault(x => x.EvaluatedInclude.EndsWith("_build.csproj"));
                if (buildProj != default)
                {
                    // Remove the build project otherwise things get *funky*
                    traversalProject.RemoveItem(buildProj);
                }
                traversalProject.Save();
            }
            else if (TargetProject?.Length > 0)
            {
                Log.Information("{@Projects} selected for build", TargetProject);
                
                var proj = new Microsoft.Build.Evaluation.Project
                {
                    Xml =
                    {
                        Sdk = traversalSdk
                    }
                };

                var projectsToBuild = new HashSet<string>();
                var additionalProjectNames = new HashSet<string>();

                void AddProjectsToBuild(string projectName)
                {
                    var project = Solution.GetProject(projectName);

                    if (project == null)
                    {
                        var ex = new FileNotFoundException($"Project {projectName} not found in solution {Solution.Name}");
                        Log.Fatal(ex, "Project {ProjectName} not found", projectName);
                        throw ex;
                    }

                    if (project.Name.EndsWith("_build"))
                    {
                        Log.Debug("SKipping NUKE project, otherwise things get *funky*");
                        return;
                    }
                    
                    projectsToBuild.Add(project.Path);

                    var msProj = project.GetMSBuildProject();
                    
                    foreach (var x in msProj.GetItems("ProjectReference")
                                 .Select(x => x.EvaluatedInclude))
                    {
                        var name = Path.GetFileNameWithoutExtension(x);
                        additionalProjectNames.Add(name);
                        AddProjectsToBuild(name);
                    }
                }
                
                foreach (var projectName in TargetProject)
                {
                    AddProjectsToBuild(projectName);
                }

                Log.Information("Additional projects added to build due to referencing: {@Projects}", additionalProjectNames);
                
                foreach (var key in projectsToBuild)
                {
                    proj.AddItemFast("ProjectReference", key);
                }
                
                proj.Save(TraversalProject);
                
                Log.Debug("Traversal project saved to {TraversalProject}", TraversalProject);

                ProjectCollection.GlobalProjectCollection.TryUnloadProject(proj.Xml);
            }
            else
            {
                var ex = new ArgumentException("No projects were specified for build");
                Log.Fatal(ex, "No projects specified for build, please specify either --build-all, --build-changed or --target-project");
                throw ex;
            }
        });
}