using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Serilog;

// ReSharper disable CheckNamespace

public partial class ConventionalCommitParser
{
	public ConventionalCommit Parse(Commit commit)
	{
		Log.Debug("Parsing commit {Sha}", commit.Sha);
		Log.Verbose("Commit message: {Message}", commit.Message);
		var lines = commit.Message.Split('\n');

		Log.Verbose("{LineCount} lines in commit message", lines.Length);
		
		var headerMatch = HeaderPattern().Match(lines[0]);

		Log.Verbose("Header: {@Header}", headerMatch.Groups);
		
		var sbBody = new StringBuilder();

		var footer = new Dictionary<string, string>();

		if (lines.Length > 1)
		{
			foreach (var s in lines.Skip(1))
			{
				Log.Verbose("Processing line '{Line}'", s);
				if (string.IsNullOrWhiteSpace(s))
				{
					Log.Verbose("Skipping empty line");
					continue;
				}
				
				var footerMatch = FooterPattern().Match(s);
				if (footerMatch.Success)
				{
					Log.Verbose("Line is a footer, adding to footer dictionary");
					footer[footerMatch.Groups["key"].Value] = footerMatch.Groups["value"].Value;
				}
				else
				{
					Log.Verbose("Line is not a footer, adding to body");
					sbBody.AppendLine(s);
				}
			}
		}

		var isBreaking = footer.ContainsKey("BREAKING CHANGE") || footer.ContainsKey("BREAKING-CHANGE") ||
		                 headerMatch.Groups["breaking"].Success;

		
		var cCommit = new ConventionalCommit(commit.Sha, headerMatch.Groups["type"].Value, headerMatch.Groups["scope"].Value, isBreaking,
			headerMatch.Groups["subject"].Value, sbBody.ToString(), footer);

		Log.Debug("Parsed commit {Sha} as {@Commit}", commit.Sha, cCommit);
		
		return cCommit;
	}

    [GeneratedRegex("(?<type>.*?)(\\((?<scope>.*)\\))?(?<breaking>!)?: (?<subject>.*)", RegexOptions.ExplicitCapture)]
    private static partial Regex HeaderPattern();
    
    [GeneratedRegex("^(?<key>\\w*(-\\w*)*): (?<value>.*)|^(?<key>BREAKING CHANGE): (?<value>.*)", RegexOptions.ExplicitCapture)]
    private static partial Regex FooterPattern();
}