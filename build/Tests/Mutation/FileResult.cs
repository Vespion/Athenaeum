using System.Text.Json.Serialization;

/// <summary>
/// Mutated file, with the relative path of the file as the key.
/// </summary>
public partial class FileResult
{
	/// <summary>
	/// Programming language that is used. Used for code highlighting, see
	/// https://prismjs.com/#examples.
	/// </summary>
	[JsonPropertyName("language")]
	public string Language { get; set; }

	[JsonPropertyName("mutants")]
	public MutantResult[] Mutants { get; set; }

	/// <summary>
	/// Full source code of the original file (without mutants), this is used to display exactly
	/// what was changed for each mutant.
	/// </summary>
	[JsonPropertyName("source")]
	public string Source { get; set; }
}