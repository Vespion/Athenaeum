using System.Text.Json.Serialization;

/// <summary>
/// Single mutation.
/// </summary>
public partial class MutantResult
{
	/// <summary>
	/// The test ids that covered this mutant. If a mutation testing framework doesn't measure
	/// this information, it can simply be left out.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("coveredBy")]
	public string[] CoveredBy { get; set; }

	/// <summary>
	/// Description of the applied mutation.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("description")]
	public string Description { get; set; }

	/// <summary>
	/// The net time it took to test this mutant in milliseconds. This is the time measurement
	/// without overhead from the mutation testing framework.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("duration")]
	public double? Duration { get; set; }

	/// <summary>
	/// Unique id, can be used to correlate this mutant across reports.
	/// </summary>
	[JsonPropertyName("id")]
	public string Id { get; set; }

	/// <summary>
	/// The test ids that killed this mutant. It is a best practice to "bail" on first failing
	/// test, in which case you can fill this array with that one test.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("killedBy")]
	public string[] KilledBy { get; set; }

	[JsonPropertyName("location")]
	public Location Location { get; set; }

	/// <summary>
	/// Category of the mutation.
	/// </summary>
	[JsonPropertyName("mutatorName")]
	public string MutatorName { get; set; }

	/// <summary>
	/// Actual mutation that has been applied.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("replacement")]
	public string Replacement { get; set; }

	/// <summary>
	/// A static mutant means that it was loaded once at during initialization, this makes it
	/// slow or even impossible to test, depending on the mutation testing framework.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("static")]
	public bool? Static { get; set; }

	/// <summary>
	/// Result of the mutation.
	/// </summary>
	[JsonPropertyName("status")]
	public MutantStatus Status { get; set; }

	/// <summary>
	/// The reason that this mutant has this status. In the case of a killed mutant, this should
	/// be filled with the failure message(s) of the failing tests. In case of an error mutant,
	/// this should be filled with the error message.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("statusReason")]
	public string StatusReason { get; set; }

	/// <summary>
	/// The number of tests actually completed in order to test this mutant. Can differ from
	/// "coveredBy" because of bailing a mutant test run after first failing test.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("testsCompleted")]
	public double? TestsCompleted { get; set; }
}