namespace VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;

/// <summary>
///     Contract for a factory that creates storage plugin instances.
/// </summary>
public interface IStorageFactoryPlugin
{
	/// <summary>
	///     Creates an instance of a storage plugin. This method opens an existing storage area.
	/// </summary>
	/// <param name="configuration">The configuration needed to connect to this storage area</param>
	/// <returns>The storage plugin</returns>
	ValueTask<IStoragePlugin> OpenAsync(IDictionary<string, string> configuration);

	/// <summary>
	///     Creates an instance of a storage plugin. This method initialises a new storage area.
	/// </summary>
	/// <returns>A tuple containing the storage plugin and the required configuration to open this plugin in the future</returns>
	ValueTask<(IDictionary<string, string> configuration, IStoragePlugin)> CreateAsync();

	/// <summary>
	///     Deletes an existing storage area.
	/// </summary>
	/// <param name="configuration">The configuration needed to connect to this storage area</param>
	ValueTask DeleteAsync(IDictionary<string, string> configuration);
}

/// <summary>
///     Contract for a storage plugin.
/// </summary>
/// <remarks>
///     Storage plugins allow Athenaeum to store data in a loosely coupled manner such as on filesystems and cloud storage.
/// </remarks>
public interface IStoragePlugin : IDisposable
{
	/// <summary>
	///     A prefix that can be added to a path to indicate that the path is a scratch path.
	/// </summary>
	/// <remarks>
	///     A scratch path is intended for temporary data that can be deleted at any time. This is useful for plugins that need
	///     to cache data or temporally persist state
	/// </remarks>
	string ScratchPathPrefix { get; }

	/// <summary>
	///     Gets a raw stream of data. Primarily intended for binary data
	/// </summary>
	/// <param name="path">The key of the data to retrieve</param>
	/// <returns>A binary stream of the data, this stream should be read only</returns>
	ValueTask<Stream> GetStreamAsync(string path);

	/// <summary>
	///     Saves a raw stream of data. Primarily intended for binary data
	/// </summary>
	/// <param name="path">The key of the data to save</param>
	/// <param name="stream">The data stream to save</param>
	/// <param name="category">The type of data, some plugins may choose to store categories differently from each other</param>
	ValueTask SaveStreamAsync(string path, Stream stream, DataCategory category);

	/// <summary>
	///     Get a data object
	/// </summary>
	/// <param name="path">The key of the data to retrieve</param>
	/// <typeparam name="T">The type of the object, must inherit from <see cref="IDataObject" /></typeparam>
	/// <returns>The resolved object</returns>
	ValueTask<T?> GetAsync<T>(string path) where T : IDataObject;

	/// <summary>
	///     Saves a data object
	/// </summary>
	/// <param name="path">The key of the data to save</param>
	/// <param name="obj">The object to save</param>
	/// <param name="category">The type of data, some plugins may choose to store categories differently from each other</param>
	ValueTask SaveAsync<T>(string path, T obj, DataCategory category) where T : IDataObject;

	/// <summary>
	///     Checks if a data object exists
	/// </summary>
	/// <param name="path">The key to check</param>
	/// <returns>True if the data object exists, else false</returns>
	ValueTask<bool> Exists(string path);
}