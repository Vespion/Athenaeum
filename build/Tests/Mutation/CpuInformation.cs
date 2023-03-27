using System.Text.Json.Serialization;

public partial class CpuInformation
{
	/// <summary>
	/// Clock speed in MHz
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("baseClock")]
	public double? BaseClock { get; set; }

	[JsonPropertyName("logicalCores")]
	public double LogicalCores { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("model")]
	public string Model { get; set; }
}