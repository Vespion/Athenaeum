using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using VespionSoftworks.Athenaeum.Plugins.Abstractions.HostServices.Prompt;
using Xunit;
using Xunit.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests;

public class TestPrompter: IPrompter
{
	private readonly Queue<IDictionary<string, object>>? _responses;
	private readonly Queue<bool>? _confirmations;

	public event EventHandler<ICollection<Prompt>>? PromptsRequested; 
	public event EventHandler<(string title, string message, bool destructive)>? ConfirmationRequested; 

	public TestPrompter(Queue<IDictionary<string, object>>? responses = null, Queue<bool>? confirmations = null)
	{
		_responses = responses;
		_confirmations = confirmations;
	}

	/// <inheritdoc />
	public ValueTask<IDictionary<string, object>> PromptAsync(ICollection<Prompt> prompts)
	{
		PromptsRequested?.Invoke(this, prompts);
		return ValueTask.FromResult(_responses?.Dequeue() ?? throw new NullReferenceException());
	}

	/// <inheritdoc />
	public ValueTask<bool> PromptConfirmationAsync(string title, string message, bool destructive = false)
	{
		ConfirmationRequested?.Invoke(this, (title, message, destructive));
		return ValueTask.FromResult(_confirmations?.Dequeue() ?? throw new NullReferenceException());
	}
}

public class Factory
{
	private readonly ITestOutputHelper _output;

	public Factory(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public async Task OpensSuccessfully()
	{
		var prompter = new TestPrompter();
		var fs = new MockFileSystem();
		
		fs.AddDirectory("test");
		
		var fact = new StorageFactory(prompter, fs, _output.ToLogger<StoragePlugin>());

		var config = new Dictionary<string, string>()
		{
			{ "path", "test" }
		};

		var plugin = await fact.OpenAsync(config);
		plugin.Should().NotBeNull();
	}
	
	[Fact]
	public async Task OpensMissingSuccessfully()
	{
		var prompter = new TestPrompter();
		var fs = new MockFileSystem();
		
		var fact = new StorageFactory(prompter, fs, _output.ToLogger<StoragePlugin>());

		var config = new Dictionary<string, string>()
		{
			{ "path", "test" }
		};

		var plugin = await fact.OpenAsync(config);
		plugin.Should().NotBeNull();

		var expectedPath = fs.Path.GetFullPath("test");
		
		fs.AllDirectories.Should().Contain(expectedPath);
	}
	
	[Fact]
	public async Task CreatesSuccessfully()
	{
		var responses = new Queue<IDictionary<string, object>>(new[]
		{
			new Dictionary<string, object>
			{
				{"Directory", "test"}
			}
		});
		
		var prompter = new TestPrompter(responses);
		var fs = new MockFileSystem();
		
		var fact = new StorageFactory(prompter, fs, _output.ToLogger<StoragePlugin>());

		prompter.PromptsRequested += (_, prompts) =>
		{
			prompts.Should()
				.ContainSingle(p =>
					p.Description == "The directory to store the files in" && p.Type == PromptTypes.DirectoryPath &&
					p.Name == "Directory");
		};
		
		var (configuration, storagePlugin) = await fact.CreateAsync();
		
		configuration.Should().ContainKey("path").WhoseValue.Should().Be("test");
		storagePlugin.Should().NotBeNull();
	}

	[Fact]
	public async Task DoesNotConfirmWhenDirExistsButIsEmpty()
	{
		var responses = new Queue<IDictionary<string, object>>(new[]
		{
			new Dictionary<string, object>
			{
				{"Directory", "test"}
			}
		});
		
		var prompter = new TestPrompter(responses);
		var fs = new MockFileSystem();

		prompter.ConfirmationRequested += (_, _) => Assert.Fail("Confirmation should not be requested");
		fs.AddDirectory("test");
		
		var fact = new StorageFactory(prompter, fs, _output.ToLogger<StoragePlugin>());
		
		var (configuration, storagePlugin) = await fact.CreateAsync();
		
		configuration.Should().ContainKey("path").WhoseValue.Should().Be("test");
		storagePlugin.Should().NotBeNull();
	}
	
	[Fact]
	public async Task ConfirmsWhenDirExists()
	{
		var responses = new Queue<IDictionary<string, object>>(new[]
		{
			new Dictionary<string, object>
			{
				{"Directory", "test"}
			}
		});
		
		var confirmations = new Queue<bool>(new[] {true});
		
		var prompter = new TestPrompter(responses, confirmations);
		var fs = new MockFileSystem();
		fs.AddFile("test/r/temp.txt", new MockFileData("test data"));
		
		var fact = new StorageFactory(prompter, fs, _output.ToLogger<StoragePlugin>());
		
		
		var prompterCalled = false;
		prompter.ConfirmationRequested += (_, args) =>
		{
			args.title.Should().Be("Directory Exists");
			args.message.Should().Be("The directory 'test' already contains files. Do you want to continue?");
			args.destructive.Should().BeTrue();
			prompterCalled = true;
		};
		
		var (configuration, storagePlugin) = await fact.CreateAsync();
		
		configuration.Should().ContainKey("path").WhoseValue.Should().Be("test");
		storagePlugin.Should().NotBeNull();
		prompterCalled.Should().BeTrue();
	}
	
	[Fact]
	public async Task ConfirmsWhenDirExistsExitsIfNoPermission()
	{
		var responses = new Queue<IDictionary<string, object>>(new[]
		{
			new Dictionary<string, object>
			{
				{"Directory", "test"}
			}
		});
		
		var confirmations = new Queue<bool>(new[] {false});
		
		var prompter = new TestPrompter(responses, confirmations);
		var fs = new MockFileSystem();
		fs.AddFile("test/temp.txt", new MockFileData("test data"));
		
		var fact = new StorageFactory(prompter, fs, _output.ToLogger<StoragePlugin>());

		var t = async () => _ = await fact.CreateAsync();
		await t.Should()
			.ThrowExactlyAsync<UnauthorizedAccessException>();
	}
	
	[Fact]
	public async Task DeletesSuccessfullyWhenConfirmed()
	{
		var confirmation = new Queue<bool>(new[] {true});
		var prompter = new TestPrompter(null, confirmation);
		var fs = new MockFileSystem();
		
		fs.AddDirectory("test");
		fs.AddFile("test/r/temp.txt", new MockFileData("test data"));
		
		var fact = new StorageFactory(prompter, fs, _output.ToLogger<StoragePlugin>());

		var config = new Dictionary<string, string>()
		{
			{ "path", "test" }
		};

		prompter.ConfirmationRequested += (_, args) =>
		{
			args.title.Should().Be("Delete directory");
			args.message.Should().Be("Are you sure you want to delete the directory 'test'?");
			args.destructive.Should().BeTrue();
		};
		
		await fact.DeleteAsync(config);

		var expectedPath = fs.Path.GetFullPath("test");
		fs.AllDirectories.Should().NotContain(expectedPath);
	}
	
	[Fact]
	public async Task DoesNotDeleteWhenNotConfirmed()
	{
		var confirmation = new Queue<bool>(new[] {false});
		var prompter = new TestPrompter(null, confirmation);
		var fs = new MockFileSystem();
		
		fs.AddDirectory("test");
		
		var fact = new StorageFactory(prompter, fs, _output.ToLogger<StoragePlugin>());

		var config = new Dictionary<string, string>()
		{
			{ "path", "test" }
		};

		await fact.DeleteAsync(config);
		var expectedPath = fs.Path.GetFullPath("test");
		fs.AllDirectories.Should().Contain(expectedPath);
	}
}