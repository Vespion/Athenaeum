using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record PluginConfiguration: IValidatableObject
{
	public string PluginDirectory { get; init; } = null!;
	public IReadOnlyList<NuGetFeed> Feeds { get; init; } = null!;
	public IReadOnlyList<NuGetPackage> Plugins { get; init; } = Array.Empty<NuGetPackage>();

	/// <inheritdoc />
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		var failures = new List<ValidationResult>(2);
		if (string.IsNullOrWhiteSpace(PluginDirectory))
		{
			failures.Add(new ValidationResult("Plugin directory is required", new[] { nameof(PluginDirectory) }));
		}
		else
		{
			Directory.CreateDirectory(PluginDirectory);
		}

		if (Feeds is not { Count: not 0 })
		{
			failures.Add(new ValidationResult("At least one feed is required", new[] { nameof(Feeds) }));
		}

		return failures;
	}
}