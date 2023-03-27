using System.Text.Json.Serialization;

/// <summary>
/// Information about the system that performed mutation testing.
/// </summary>
public partial class SystemInformation
{
	/// <summary>
	/// Did mutation testing run in a Continuous Integration environment (pipeline)? Note that
	/// there is no way of knowing this for sure. It's done on a best-effort basis.
	/// </summary>
	[JsonPropertyName("ci")]
	public bool Ci { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("cpu")]
	public CpuInformation Cpu { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("os")]
	public OsInformation Os { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("ram")]
	public RamInformation Ram { get; set; }
}