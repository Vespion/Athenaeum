using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using VespionSoftworks.Athenaeum.Plugins.Abstractions.HostServices.Prompt;
using VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem;

/// <inheritdoc />
public sealed class StorageFactory : IStorageFactoryPlugin
{
	private const string PathConfigKey = "path";
	private readonly IFileSystem _fileSystem;
	private readonly ILogger<StoragePlugin> _pluginLogger;
	private readonly IPrompter _prompter;

	// ReSharper disable once ContextualLoggerProblem
	public StorageFactory(IPrompter prompter, IFileSystem fileSystem, ILogger<StoragePlugin> pluginLogger)
	{
		_prompter = prompter;
		_fileSystem = fileSystem;
		_pluginLogger = pluginLogger;
	}

	/// <inheritdoc />
	public ValueTask<IStoragePlugin> Open(IDictionary<string, string> configuration)
	{
		var path = configuration[PathConfigKey];

		var root = _fileSystem.DirectoryInfo.New(path);
		if (!root.Exists) root.Create();

		return new ValueTask<IStoragePlugin>(new StoragePlugin(root, _pluginLogger));
	}

	/// <inheritdoc />
	public async ValueTask<(IDictionary<string, string> configuration, IStoragePlugin)> Create()
	{
		var fileLocation = await _prompter.PromptForDirectoryAsync("The directory to store the files in");
		var config = new Dictionary<string, string>
		{
			{ PathConfigKey, fileLocation }
		};

		var plugin = await Open(config);

		return (config, plugin);
	}

	/// <inheritdoc />
	public async ValueTask Delete(IDictionary<string, string> configuration)
	{
		if (!await _prompter.PromptConfirmationAsync("Delete storage",
			    "Are you sure you want to delete this storage?",
			    true)) return;
		var path = configuration[PathConfigKey];
		_fileSystem.Directory.Delete(path, true);
	}
}