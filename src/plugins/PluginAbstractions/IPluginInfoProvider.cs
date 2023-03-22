namespace VespionSoftworks.Athenaeum.Plugins.Abstractions;

public interface IPluginInfoProvider
{
	string Name { get; }
	string? Description { get; }
	string Author { get; }
	string? MinimumHostVersion { get; }
	string? MaximumHostVersion { get; }
}