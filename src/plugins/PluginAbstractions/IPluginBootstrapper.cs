using Microsoft.Extensions.DependencyInjection;

namespace PluginAbstractions;

public interface IPluginBootstrapper
{
	public void ConfigureServices(IServiceCollection service);
}