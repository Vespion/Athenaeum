using System.Text.Json.Serialization;

/// <summary>
/// The performance statistics per phase. Total time should be roughly equal to the sum of
/// all these.
/// </summary>
public partial class PerformanceStatistics
{
	/// <summary>
	/// Time it took to run the initial test phase (dry-run) in milliseconds.
	/// </summary>
	[JsonPropertyName("initialRun")]
	public double InitialRun { get; set; }

	/// <summary>
	/// Time it took to run the mutation test phase in milliseconds.
	/// </summary>
	[JsonPropertyName("mutation")]
	public double Mutation { get; set; }

	/// <summary>
	/// Time it took to run the setup phase in milliseconds.
	/// </summary>
	[JsonPropertyName("setup")]
	public double Setup { get; set; }
}