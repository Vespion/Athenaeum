namespace VespionSoftworks.Athenaeum.Plugins.Abstractions.HostServices.Prompt;

/// <summary>
///     A broker to allow plugins to prompt the user for input
/// </summary>
public interface IPrompter
{
	ValueTask<IDictionary<string, object>> PromptAsync(ICollection<Prompt> prompts);

	ValueTask<bool> PromptConfirmationAsync(string title, string message, bool destructive = false);
}

public static class PrompterExtensions
{
	/// <summary>
	///     Prompts the user for their username and password
	/// </summary>
	/// <returns>The user name and password in a identity object</returns>
	public static async ValueTask<(string Username, string Password)> PromptBasicAuthenticationAsync(
		this IPrompter prompter)
	{
		const string username = "Username";
		const string password = "Password";
		var prompts = new List<Prompt>
		{
			new Prompt("Your username", username, PromptTypes.String),
			new Prompt("Your password", password, PromptTypes.SecureString)
		};

		var results = await prompter.PromptAsync(prompts);

		return new ValueTuple<string, string>(
			results[username].ToString() ?? throw new ArgumentNullException(username, "Username cannot be null"),
			results[password].ToString() ?? throw new ArgumentNullException(password, "Password cannot be null")
		);
	}

	public static async ValueTask<string> PromptForDirectoryAsync(this IPrompter prompter, string description)
	{
		const string directory = "Directory";

		var prompts = new List<Prompt>
		{
			new Prompt(description, directory, PromptTypes.DirectoryPath)
		};

		var results = await prompter.PromptAsync(prompts);

		return results[directory].ToString() ??
		       throw new ArgumentNullException(directory, "Directory path cannot be null");
	}
}