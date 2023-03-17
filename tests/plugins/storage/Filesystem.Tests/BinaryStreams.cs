using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests;

public class BinaryStreams
{
	private const string Text =
		"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
	private readonly ITestOutputHelper _output;

	public BinaryStreams(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public async Task SavesStreamSuccessfully()
	{
		var (plugin, fs, log) = Helpers.CreatePlugin(_output);
		
		using var stream = new MemoryStream();
		await using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
		{
			await streamWriter.WriteAsync(Text);
		}
		stream.Seek(0, SeekOrigin.Begin);
		
		await plugin.SaveStreamAsync("stream", stream, DataCategory.Binary);

		fs.FileExists("stream").Should().BeTrue();
		
		log.RecordedLogs.Should().HaveCount(4);
		log.RecordedDebugLogs.Should().HaveCount(2)
			.And.HaveElementAt(0, (LogLevel.Debug, null, "Saving binary stream for 'stream'"))
			.And.HaveElementAt(1, (LogLevel.Debug, null, "Writing stream to file"));
	}
	
	[Fact]
	public async Task ReadsStreamSuccessfully()
	{
		var (plugin, fs, log) = Helpers.CreatePlugin(_output);
		
		fs.AddFile("stream", new MockFileData(Text));
		
		await using var stream = await plugin.GetStreamAsync("stream");
		using var streamReader = new StreamReader(stream, leaveOpen: false);
		
		var result = await streamReader.ReadToEndAsync();
		Assert.Equal(Text, result);
		
		log.RecordedLogs.Should().HaveCount(3);
		log.RecordedDebugLogs.Should().ContainSingle(x => "Loading binary stream for 'stream'" == x.Message);
	}
	
	[Fact]
	public async Task RoundtripStreamSuccessfully()
	{
		var (plugin, _, _) = Helpers.CreatePlugin(_output);
		
		using var writeStream = new MemoryStream();
		await using (var streamWriter = new StreamWriter(writeStream, leaveOpen: true))
		{
			await streamWriter.WriteAsync(Text);
		}
		writeStream.Seek(0, SeekOrigin.Begin);
		
		await plugin.SaveStreamAsync("stream", writeStream, DataCategory.Binary);

		await using var readStream = await plugin.GetStreamAsync("stream");
		using var streamReader = new StreamReader(readStream, leaveOpen: false);
		var result = await streamReader.ReadToEndAsync();
		Assert.Equal(Text, result);
	}
}