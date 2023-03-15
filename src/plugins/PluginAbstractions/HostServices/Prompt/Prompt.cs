namespace VespionSoftworks.Athenaeum.Plugins.Abstractions.HostServices.Prompt;

/// <summary>
///     A prompt to be displayed to the user
/// </summary>
public readonly struct Prompt
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Prompt"/> struct.
	/// </summary>
	/// <param name="description">A description used to explain why the data for this prompt is needed</param>
	/// <param name="name">A name that will be used in the returned results, and may be displayed to the user</param>
	/// <param name="type">The type of prompt requested</param>
	public Prompt(string description, string name, PromptTypes type)
	{
		Name = name;
		Description = description;
		Type = type;
	}
		
	public string Name { get; }
	public string Description { get; }
	public PromptTypes Type { get; }
}