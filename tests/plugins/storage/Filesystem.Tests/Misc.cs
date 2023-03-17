using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests;

public class Misc
{
	private readonly ITestOutputHelper _output;

	public Misc(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public async Task FindsExistingFile()
	{
		var (plugin, fs, log) = Helpers.CreatePlugin(_output);

		fs.AddFile("obj", new MockFileData("test data"));

		var exists = await plugin.Exists("obj");
		Assert.True(exists);
		log.RecordedLogs.Should().HaveCount(3);
		log.RecordedDebugLogs.Should().ContainSingle(x => "Checking if there is a file for 'obj'" == x.Message);
	}
	
	[Fact]
	public async Task DoesNotFindMissingFile()
	{
		var (plugin, _, log) = Helpers.CreatePlugin(_output);
		
		var exists = await plugin.Exists("obj");
		Assert.False(exists);
		log.RecordedLogs.Should().HaveCount(3);
		log.RecordedDebugLogs.Should().ContainSingle(x => "Checking if there is a file for 'obj'" == x.Message);
	}

	[Fact]
	public void LogsDisposal()
	{
		var (plugin, _, log) = Helpers.CreatePlugin(_output);
		
		plugin.Dispose();

		log.RecordedLogs
			.Should()
			.ContainSingle(x => x.Level == LogLevel.Trace && x.Message == "Disposal triggered");
	}
	
	[Fact]
	public void ResolvesPath()
	{
		var (plugin, _, log) = Helpers.CreatePlugin(_output);
		
		var path = plugin.ResolveFullPath("obj");
		path.Should().Be("/obj");

		log.RecordedLogs.Should().HaveCount(2);
		var logs = log.RecordedTraceLogs.ToArray();
		logs.Should().HaveCount(2);
		logs[0].Message.Should().Be("Resolving full path for 'obj'");
		logs[1].Message.Should().Be("Resolved full path for 'obj' to '/obj'");
	}
}