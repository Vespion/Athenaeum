namespace VespionSoftworks.Athenaeum.Plugins.Storage.Abstractions;

/// <summary>
///     Implement this interface to allow the storage plugin to connect to an authenticated service.
/// </summary>
public interface IAuthenticatedStoragePlugin
{
	/// <summary>
	///     Triggers an authentication process for the storage plugin
	/// </summary>
	ValueTask AuthenticateAsync();
}