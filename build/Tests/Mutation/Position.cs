using System.Text.Json.Serialization;

/// <summary>
/// Position of a mutation. Both line and column start at one.
/// </summary>
public partial class Position
{
	[JsonPropertyName("column")]
	public long Column { get; set; }

	[JsonPropertyName("line")]
	public long Line { get; set; }
}