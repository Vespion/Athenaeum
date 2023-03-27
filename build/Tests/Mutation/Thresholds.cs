using System.Text.Json.Serialization;

/// <summary>
/// Thresholds for the status of the reported application.
/// </summary>
public partial class Thresholds
{
	/// <summary>
	/// Higher bound threshold.
	/// </summary>
	[JsonPropertyName("high")]
	public long High { get; set; }

	/// <summary>
	/// Lower bound threshold.
	/// </summary>
	[JsonPropertyName("low")]
	public long Low { get; set; }
}