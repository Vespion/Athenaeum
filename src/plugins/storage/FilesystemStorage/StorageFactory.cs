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
	public ValueTask<IStoragePlugin> OpenAsync(IDictionary<string, string> configuration)
	{
		var path = configuration[PathConfigKey];

		var root = _fileSystem.DirectoryInfo.New(path);
		if (!root.Exists) root.Create();

		return new ValueTask<IStoragePlugin>(new StoragePlugin(root, _pluginLogger));
	}

	/// <inheritdoc />
	public async ValueTask<(IDictionary<string, string> configuration, IStoragePlugin)> CreateAsync()
	{
		var fileLocation = await _prompter.PromptForDirectoryAsync("The directory to store the files in");
		var config = new Dictionary<string, string>
		{
			{ PathConfigKey, fileLocation }
		};

		var dirInfo = _fileSystem.DirectoryInfo.New(fileLocation);
		
		if (dirInfo.Exists)
		{
			//Directory already exists, check for files
			if (dirInfo.EnumerateFiles("", SearchOption.AllDirectories).Any())
			{
				var okay = await _prompter.PromptConfirmationAsync("Directory Exists",
					$"The directory '{fileLocation}' already contains files. Do you want to continue?",
					true);

				if (!okay)
				{
					throw new UnauthorizedAccessException();
				}
			}
		}
		
		var plugin = await OpenAsync(config);

		return (config, plugin);
	}

	/// <inheritdoc />
	public async ValueTask DeleteAsync(IDictionary<string, string> configuration)
	{
		var path = configuration[PathConfigKey];
		if (!await _prompter.PromptConfirmationAsync("Delete directory",
			    $"Are you sure you want to delete the directory '{path}'?",
			    true)) return;
		_fileSystem.Directory.Delete(path, true);
	}
}