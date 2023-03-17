using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;

[assembly: InternalsVisibleTo("VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests")]
namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem;

/// <inheritdoc />
public sealed class StoragePlugin : IStoragePlugin
{
	private readonly ILogger<StoragePlugin> _logger;
	private readonly IDirectoryInfo _root;

	public StoragePlugin(IDirectoryInfo root, ILogger<StoragePlugin> logger)
	{
		_root = root;
		_logger = logger;
	}

	/// <inheritdoc />
	public ValueTask<Stream> GetStreamAsync(string path)
	{
		_logger.LogDebug("Loading binary stream for '{Path}'", path);
		var fullPath = ResolveFullPath(path);
		return new ValueTask<Stream>(_root.FileSystem.File.Open(fullPath, FileMode.Open, FileAccess.Read,
			FileShare.None));
	}

	/// <inheritdoc />
	public async ValueTask SaveStreamAsync(string path, Stream stream, DataCategory category)
	{
		_logger.LogDebug("Saving binary stream for '{Path}'", path);
		var fullPath = ResolveFullPath(path);
		var fileInfo = _root.FileSystem.FileInfo.New(fullPath);

		//Write to file
		_logger.LogDebug("Writing stream to file");
		await using var fileStream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.None);
		await stream.CopyToAsync(fileStream);
		fileInfo.Attributes = FileAttributes.NotContentIndexed;
	}

	/// <inheritdoc />
	public async ValueTask<T?> GetAsync<T>(string path) where T : IDataObject
	{
		_logger.LogDebug("Loading object ({Type}) for '{Path}'", typeof(T), path);
		var fullPath = ResolveFullPath(path);
		var fileInfo = _root.FileSystem.FileInfo.New(fullPath);

		_logger.LogDebug("Deserializing object");
		await using var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
		try
		{
			var result = await JsonSerializer.DeserializeAsync<T>(fileStream);

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to deserialize object to {Type}", typeof(T).FullName);
			throw;
		}
	}

	/// <inheritdoc />
	public async ValueTask SaveAsync<T>(string path, T obj, DataCategory category) where T : IDataObject
	{
		_logger.LogDebug("Saving object ({Type}) for '{Path}'", typeof(T), path);
		var fullPath = ResolveFullPath(path);
		var fileInfo = _root.FileSystem.FileInfo.New(fullPath);

		//Write to file
		_logger.LogDebug("Serializing object");
		await using var fileStream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.None);
		await JsonSerializer.SerializeAsync(fileStream, obj, obj.GetType());
		fileInfo.Attributes = FileAttributes.NotContentIndexed;
	}

	/// <inheritdoc />
	public ValueTask<bool> Exists(string path)
	{
		_logger.LogDebug("Checking if there is a file for '{Path}'", path);
		var fullPath = ResolveFullPath(path);
		return new ValueTask<bool>(_root.FileSystem.FileInfo.New(fullPath).Exists);
	}

	/// <inheritdoc />
	public string ScratchPathPrefix => _root.FileSystem.Path.GetTempPath();

	/// <inheritdoc />
	public void Dispose()
	{
		_logger.LogTrace("Disposal triggered");
	}

	internal string ResolveFullPath(string path)
	{
		_logger.LogTrace("Resolving full path for '{Path}'", path);
		var full = _root.FileSystem.Path.GetFullPath(path);
		_logger.LogTrace("Resolved full path for '{Path}' to '{FullPath}'", path, full);
		return full;
	}
}