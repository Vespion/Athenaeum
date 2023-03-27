using System.Text.Json.Serialization;

public partial class RamInformation
{
	/// <summary>
	/// The total RAM of the system that performed mutation testing in MB.
	/// </summary>
	[JsonPropertyName("total")]
	public double Total { get; set; }
}