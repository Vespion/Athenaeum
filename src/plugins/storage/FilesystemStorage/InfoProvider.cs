using Microsoft.Extensions.Localization;
using VespionSoftworks.Athenaeum.Plugins.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem;

public class InfoProvider: IPluginInfoProvider
{
	private readonly IStringLocalizer<StoragePlugin> _localizer;

	public InfoProvider(IStringLocalizer<StoragePlugin> localizer)
	{
		_localizer = localizer;
	}

	/// <inheritdoc />
	public string Name => _localizer.GetString(nameof(Name));

	/// <inheritdoc />
	public string Description => _localizer.GetString(nameof(Description));

	/// <inheritdoc />
	public string Author => "Vespion Softworks";

	/// <inheritdoc />
	public string MinimumHostVersion => "1.0.0";

	/// <inheritdoc />
	public string? MaximumHostVersion => null;
}