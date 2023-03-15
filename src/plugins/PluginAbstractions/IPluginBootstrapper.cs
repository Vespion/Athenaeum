using Microsoft.Extensions.DependencyInjection;

namespace VespionSoftworks.Athenaeum.Plugins.Abstractions;

/// <summary>
///     Allows a plugin to configure itself and its services
/// </summary>
public interface IPluginBootstrapper
{
	/// <summary>
	///     Allows plugins to inject their own services into the DI container
	/// </summary>
	/// <param name="service">The service container</param>
	void ConfigureServices(IServiceCollection service);
}