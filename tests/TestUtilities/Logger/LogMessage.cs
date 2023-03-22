using Microsoft.Extensions.Logging;

namespace VespionSoftworks.Athenaeum.TestUtilities.Logger;

public readonly record struct LogMessage(LogLevel? Level, string Message,
	IReadOnlyDictionary<string, object>? Scopes = null, Exception? Exception = null, object? State = null,
	EventId? EventId = null)
{
	public IReadOnlyDictionary<string, object> Scopes { get; init; } = Scopes ?? new Dictionary<string, object>();
}