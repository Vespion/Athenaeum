using NuGet.Packaging.Signing;
using NuGet.Versioning;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public record PluginHeader(NuGetVersion Version, Signature? Signature);
public record PluginPackage(PluginHeader Header)
{
	public Type InfoProvider { get; internal set; } = null!;
	public ICollection<Type> Bootstrappers { get; internal set; } = new List<Type>();

	public ICollection<Type> StoragePlugins { get; internal set; } = new List<Type>();

	public ICollection<Type> StorageFactoryPlugins { get; internal set; } = new List<Type>();
}