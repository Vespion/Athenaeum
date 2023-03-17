using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public record NuGetFeed
{
	[Required]
	public string Name { get; init; } = null!;

	[Required, Url]
	public string Url { get; init; } = null!;
}