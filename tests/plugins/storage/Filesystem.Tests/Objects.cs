using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests;

public class TestDataObject : IDataObject
{
	public TestDataObject(string contents)
	{
		Contents = contents;
	}
	public string Contents { get; init; }
}

public class Objects
{
	private const string Text =
		"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
	private readonly ITestOutputHelper _output;

	public Objects(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public async Task SavesObjectSuccessfully()
	{
		var (plugin, _, log) = Helpers.CreatePlugin(_output);
		
		var dataObject = new TestDataObject(Text);

		await plugin.SaveAsync("obj", dataObject, DataCategory.DenseText);
		
		log.RecordedLogs.Should().HaveCount(4);
		log.RecordedDebugLogs.Should().HaveCount(2)
			.And.HaveElementAt(0, (LogLevel.Debug, null, "Saving object (VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests.TestDataObject) for 'obj'"))
			.And.HaveElementAt(1, (LogLevel.Debug, null, "Serializing object"));
	}
	
	[Fact]
	public async Task ReadsObjectSuccessfully()
	{
		var (plugin, fs, log) = Helpers.CreatePlugin(_output);
		
		var dataObject = new TestDataObject(Text);
		
		fs.AddFile("obj", new MockFileData(JsonSerializer.Serialize(dataObject)));
		
		var result = await plugin.GetAsync<TestDataObject>("obj");
		Assert.Equal(Text, result?.Contents);
		
		log.RecordedLogs.Should().HaveCount(4);
		log.RecordedDebugLogs.Should().HaveCount(2)
			.And.HaveElementAt(0, (LogLevel.Debug, null, "Loading object (VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests.TestDataObject) for 'obj'"))
			.And.HaveElementAt(1, (LogLevel.Debug, null, "Deserializing object"));
	}
	
	[Fact]
	public async Task RoundtripObjectSuccessfully()
	{
		var (plugin, _, _) = Helpers.CreatePlugin(_output);
		
		var dataObject = new TestDataObject(Text);

		await plugin.SaveAsync("obj", dataObject, DataCategory.DenseText);
		
		var result = await plugin.GetAsync<TestDataObject>("obj");
		Assert.Equal(Text, result?.Contents);
	}

	[Fact]
	public async Task ThrowsIfJsonInvalid()
	{
		var (plugin, fs, log) = Helpers.CreatePlugin(_output);
		
		fs.AddFile("obj", new MockFileData("{ // invalid json }"));
		
		var t = async () => await plugin.GetAsync<TestDataObject>("obj");
		await t.Should().ThrowAsync<JsonException>();
		
		log.RecordedLogs.Should().HaveCount(5);
		log.RecordedDebugLogs.Should().HaveCount(2)
			.And.HaveElementAt(0,
				(LogLevel.Debug, null,
					"Loading object (VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests.TestDataObject) for 'obj'"))
			.And.HaveElementAt(1, (LogLevel.Debug, null, "Deserializing object"));
		log.RecordedErrorLogs.Should()
			.ContainSingle(x =>
				x.Message ==
				"Failed to deserialize object to VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests.TestDataObject");
	}

	[Fact]
	public async Task SavesObjectSuccessfullyToScratch()
	{
		var (plugin, fs, log) = Helpers.CreatePlugin(_output);
		
		var dataObject = new TestDataObject(Text);

		await plugin.SaveAsync(Path.Combine(plugin.ScratchPathPrefix, "obj"), dataObject, DataCategory.DenseText);

		var scratch = fs.FileInfo.New(Path.Combine(fs.Path.GetTempPath(), "obj"));
		Assert.True(scratch.Exists);
		
		log.RecordedLogs.Should().HaveCount(4);
		log.RecordedDebugLogs.Should().HaveCount(2)
			.And.HaveElementAt(0, (LogLevel.Debug, null, $"Saving object (VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests.TestDataObject) for '{scratch.FullName}'"))
			.And.HaveElementAt(1, (LogLevel.Debug, null, "Serializing object"));
	}
	
	[Fact]
	public async Task ReadsObjectSuccessfullyFromScratch()
	{
		var (plugin, fs, log) = Helpers.CreatePlugin(_output);
		
		var dataObject = new TestDataObject(Text);
		var expectedPath = fs.Path.GetFullPath(Path.Combine(fs.Path.GetTempPath(), "obj"));
		fs.AddFile(expectedPath, new MockFileData(JsonSerializer.Serialize(dataObject)));
		
		var result = await plugin.GetAsync<TestDataObject>(Path.Combine(plugin.ScratchPathPrefix, "obj"));
		Assert.Equal(Text, result?.Contents);
		
		log.RecordedLogs.Should().HaveCount(4);
		log.RecordedDebugLogs.Should().HaveCount(2)
			.And.HaveElementAt(0, (LogLevel.Debug, null, $"Loading object (VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests.TestDataObject) for '{expectedPath}'"))
			.And.HaveElementAt(1, (LogLevel.Debug, null, "Deserializing object"));
	}
	
	[Fact]
	public async Task RoundtripObjectSuccessfullyFromScratch()
	{
		var (plugin, _, _) = Helpers.CreatePlugin(_output);
		
		var dataObject = new TestDataObject(Text);

		await plugin.SaveAsync(Path.Combine(plugin.ScratchPathPrefix, "obj"), dataObject, DataCategory.DenseText);
		
		var result = await plugin.GetAsync<TestDataObject>(Path.Combine(plugin.ScratchPathPrefix, "obj"));
		Assert.Equal(Text, result?.Contents);
	}
}