using System;
using System.Text.Json.Serialization;

/// <summary>
/// Extra branding information about the framework used.
/// </summary>
public partial class BrandingInformation
{
	/// <summary>
	/// URL to the homepage of the framework.
	/// </summary>
	[JsonPropertyName("homepageUrl")]
	public Uri HomepageUrl { get; set; }

	/// <summary>
	/// URL to an image for the framework, can be a data URL.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("imageUrl")]
	public string ImageUrl { get; set; }
}