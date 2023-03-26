using System.Linq;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Nuke.Common.IO;
using Serilog;
using Project = Microsoft.Build.Evaluation.Project;

partial class Build
{
	void UpdateBuildProperty(AbsolutePath projectDirectory, string key, string value)
	{
		var generatedPath = projectDirectory / "obj" / "Generated.Build.g.props";

		Project proj;
		if (generatedPath.FileExists())
		{
			Log.Verbose("Generated.Build.props exists, updating...");

			var cached = ProjectCollection.GlobalProjectCollection.LoadedProjects.FirstOrDefault(x => x.FullPath == generatedPath);
			if (cached == default)
			{
				proj = Project.FromFile(generatedPath, new ProjectOptions());
			}
			else
			{
				proj = cached;
			}
		}
		else
		{
			Log.Verbose("Generated.Build.props does not exist, creating...");

			proj = new Project();
		}

		proj.SetProperty(key, value);
		proj.Save(generatedPath);
	}
}