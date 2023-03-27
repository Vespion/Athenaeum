using System.Text.Json.Serialization;

/// <summary>
/// A test in your test file.
/// </summary>
public partial class TestDefinition
{
	/// <summary>
	/// Unique id of the test, used to correlate what test killed a mutant.
	/// </summary>
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("location")]
	public OpenEndLocation Location { get; set; }

	/// <summary>
	/// Name of the test, used to display the test.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; }
}