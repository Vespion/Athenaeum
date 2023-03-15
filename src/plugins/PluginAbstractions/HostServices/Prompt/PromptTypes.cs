namespace VespionSoftworks.Athenaeum.Plugins.Abstractions.HostServices.Prompt;

public enum PromptTypes
{
	/// <summary>
	///     A normal string
	/// </summary>
	String,

	/// <summary>
	///     A true/false value
	/// </summary>
	Boolean,

	/// <summary>
	///     A numeric value
	/// </summary>
	Numeric,

	/// <summary>
	///     A normal string that will not be displayed as plain text
	/// </summary>
	/// <remarks>
	///     The data in the results is returned as a <see cref="string" /> not as a <see cref="System.Security.SecureString" />
	/// </remarks>
	SecureString,

	/// <summary>
	///     A path to a directory
	/// </summary>
	/// <remarks>
	///     The data in the results is returned as a <see cref="string" />
	/// </remarks>
	DirectoryPath,

	/// <summary>
	///     A path to a directory
	/// </summary>
	/// <remarks>
	///     The data in the results is returned as a <see cref="string" />
	/// </remarks>
	FilePath
}