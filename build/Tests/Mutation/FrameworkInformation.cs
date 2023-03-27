using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Extra information about the framework used
/// </summary>
public partial class FrameworkInformation
{
	/// <summary>
	/// Extra branding information about the framework used.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("branding")]
	public BrandingInformation Branding { get; set; }

	/// <summary>
	/// Dependencies used by the framework. Key-value pair of dependencies and their versions.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("dependencies")]
	public Dictionary<string, string> Dependencies { get; set; }

	/// <summary>
	/// Name of the framework used.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; }

	/// <summary>
	/// Version of the framework.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("version")]
	public string Version { get; set; }
}