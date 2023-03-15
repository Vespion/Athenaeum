using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VespionSoftworks.Athenaeum.Plugins.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem;

/// <inheritdoc />
public sealed class Bootstrapper : IPluginBootstrapper
{
	/// <inheritdoc />
	public void ConfigureServices(IServiceCollection service)
	{
		service.TryAddSingleton<IFileSystem, FileSystem>();
	}
}