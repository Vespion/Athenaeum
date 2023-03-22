using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace VespionSoftworks.Athenaeum.TestUtilities.Logger;

public class InMemoryLoggerProvider: ILoggerProvider, ISupportExternalScope
{
	public readonly ConcurrentDictionary<string, InMemoryLogger> Loggers =
		new(StringComparer.OrdinalIgnoreCase);
	private IExternalScopeProvider? _scopeProvider;
	
	/// <inheritdoc />
	public void Dispose()
	{
		Loggers.Clear();
	}

	/// <inheritdoc />
	public ILogger CreateLogger(string categoryName) => Loggers.GetOrAdd(categoryName, name => new InMemoryLogger(name, _scopeProvider));

	/// <inheritdoc />
	public void SetScopeProvider(IExternalScopeProvider scopeProvider)
	{
		_scopeProvider = scopeProvider;
	}
}