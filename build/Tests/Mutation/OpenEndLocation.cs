using System.Text.Json.Serialization;

/// <summary>
/// A location where "end" is not required. Start is inclusive, end is exclusive.
/// </summary>
public partial class OpenEndLocation
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("end")]
	public Position End { get; set; }

	[JsonPropertyName("start")]
	public Position Start { get; set; }
}