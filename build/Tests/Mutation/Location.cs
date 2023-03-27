using System.Text.Json.Serialization;

/// <summary>
/// A location with start and end. Start is inclusive, end is exclusive.
/// </summary>
public partial class Location
{
	[JsonPropertyName("end")]
	public Position End { get; set; }

	[JsonPropertyName("start")]
	public Position Start { get; set; }
}