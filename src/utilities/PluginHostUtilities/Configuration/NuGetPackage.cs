using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record NuGetPackage: IValidatableObject
{
	public string Name { get; init; } = null!;
	public string Version { get; init; } = null!;

	public static implicit operator PackageIdentity(NuGetPackage pkg)
	{
		return new PackageIdentity(pkg.Name, new NuGetVersion(pkg.Version));
	}

	/// <inheritdoc />
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		var failures = new List<ValidationResult>(2);
		if (string.IsNullOrWhiteSpace(Name))
		{
			failures.Add(new ValidationResult("Plugin name is required", new[] { nameof(Name) }));
		}

		if (string.IsNullOrWhiteSpace(Version))
		{
			failures.Add(new ValidationResult("Plugin version is required", new[] { nameof(Version) }));
		}
		else
		{
			if (!NuGetVersion.TryParse(Version, out _))
			{
				failures.Add(new ValidationResult("Plugin version is not a valid NuGet version", new[] { nameof(Version)}));
			}
		}

		return failures;
	}
}