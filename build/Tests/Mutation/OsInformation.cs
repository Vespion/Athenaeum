using System.Text.Json.Serialization;

public partial class OsInformation
{
	/// <summary>
	/// Human-readable description of the OS
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("description")]
	public string Description { get; set; }

	/// <summary>
	/// Platform identifier
	/// </summary>
	[JsonPropertyName("platform")]
	public string Platform { get; set; }

	/// <summary>
	/// Version of the OS or distribution
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("version")]
	public string Version { get; set; }
}