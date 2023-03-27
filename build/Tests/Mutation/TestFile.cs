using System.Text.Json.Serialization;

/// <summary>
/// A file containing one or more tests
/// </summary>
public partial class TestFile
{
	/// <summary>
	/// Full source code of the test file, this can be used to display in the report.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("source")]
	public string Source { get; set; }

	[JsonPropertyName("tests")]
	public TestDefinition[] Tests { get; set; }
}