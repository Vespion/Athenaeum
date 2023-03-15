using System.ComponentModel.DataAnnotations;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

public class PluginConfiguration: IValidatableObject
{
	public string PluginDirectory { get; set; }
	public IReadOnlyList<NuGetFeed> Feeds { get; set; }
	public IReadOnlyList<NuGetPackage> Plugins { get; set; } = Array.Empty<NuGetPackage>();

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

		if (Feeds == null || Feeds.Count == 0)
		{
			failures.Add(new ValidationResult("At least one feed is required", new[] { nameof(Feeds) }));
		}

		return failures;
	}
}