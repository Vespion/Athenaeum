using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

namespace VespionSoftworks.Athenaeum.TestUtilities.Logger;

public class InMemoryLogger: ILogger
{
	private readonly List<LogMessage> _logMessages = new List<LogMessage>();
	private readonly IExternalScopeProvider? _scopeProvider;

	public string Name { get; }
	
	public InMemoryLogger(string name, IExternalScopeProvider? scopeProvider)
	{
		_scopeProvider = scopeProvider;
		Name = name;
	}

	public IReadOnlyList<LogMessage> RecordedLogs => _logMessages.AsReadOnly();
	public IReadOnlyList<LogMessage> RecordedTraceLogs => _logMessages.AsReadOnly().Where(x => x.Level == LogLevel.Trace).ToImmutableList();
	public IReadOnlyList<LogMessage> RecordedDebugLogs => _logMessages.AsReadOnly().Where(x => x.Level == LogLevel.Debug).ToImmutableList();
	public IReadOnlyList<LogMessage> RecordedErrorLogs => _logMessages.AsReadOnly().Where(x => x.Level == LogLevel.Error).ToImmutableList();

	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		var scopes = new Dictionary<string, object>();

		_scopeProvider?.ForEachScope((x, _) =>
		{
			if (x is IEnumerable<KeyValuePair<string, object>> l)
			{
				foreach (var (key, value) in l)
				{
					scopes.Add(key, value);
				}
			}
			else if (x is KeyValuePair<string, object> p)
			{
				scopes.Add(p.Key, p.Value);
			}
			else if (x is not null)
			{
				scopes.Add("Scope", x);
			}
		}, (object)null!);
		
		_logMessages.Add(new LogMessage(logLevel,
			formatter(state, exception),
			scopes, exception, state, eventId));
	}

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel)
	{
		//This logger is always enabled
		return true;
	}

	/// <inheritdoc />
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		return _scopeProvider?.Push(state);
	}
}