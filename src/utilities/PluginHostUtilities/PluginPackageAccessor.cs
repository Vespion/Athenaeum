using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using VespionSoftworks.Athenaeum.Plugins.Abstractions;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public interface IPluginPackageAccessor
{
	IPluginInfoProvider GetInfoProviderForType(Type type);
}

public class PluginPackageAccessor : IPluginPackageAccessor
{
	private readonly IReadOnlyList<PluginPackage> _packages;
	private readonly IServiceProvider _service;
	private readonly ConcurrentDictionary<Type, int> _typeToPackageIndexCache = new();

	public PluginPackageAccessor(IEnumerable<PluginPackage> packages, IServiceProvider service)
	{
		_packages = packages.ToArray();
		_service = service;
	}
	
	public IPluginInfoProvider GetInfoProviderForType(Type type)
	{
		var index = _typeToPackageIndexCache.GetOrAdd(type, (t) =>
		{
			var found = false;
			var i = 0;
			foreach (var pluginPackage in _packages)
			{
				if (pluginPackage.InfoProvider == t)
				{
					found = true;
					break;
				}
				i++;
			}

			if (!found)
			{
				throw new KeyNotFoundException();
			}
			
			return i;
		});
		
		return (IPluginInfoProvider) _service.GetRequiredService(_packages[index].InfoProvider);
	}
}