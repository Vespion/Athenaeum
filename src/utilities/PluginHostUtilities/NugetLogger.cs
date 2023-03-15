using Microsoft.Extensions.Logging;
using NuGet.Common;
using LogLevel = NuGet.Common.LogLevel;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities;

public class NugetLogger: LoggerBase
{
	private readonly ILogger<NugetLogger> _logger;

	/// <inheritdoc />
	public NugetLogger(ILogger<NugetLogger> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override void Log(ILogMessage message)
	{
		var level = (Microsoft.Extensions.Logging.LogLevel)message.Level;
		switch (message.Level)
		{
			case LogLevel.Debug:
				level = Microsoft.Extensions.Logging.LogLevel.Debug;
				break;
			case LogLevel.Verbose:
			case LogLevel.Minimal:
				level = Microsoft.Extensions.Logging.LogLevel.Trace;
				break;
			case LogLevel.Information:
				level = Microsoft.Extensions.Logging.LogLevel.Information;
				break;
			case LogLevel.Warning:
				level = Microsoft.Extensions.Logging.LogLevel.Warning;
				break;
			case LogLevel.Error:
				level = Microsoft.Extensions.Logging.LogLevel.Error;
				break;
		}
		
		using (_logger.BeginScope(new Dictionary<string, object> { { "nuget_code", message.Code } }))
		{
			if(message.Code > NuGetLogCode.Undefined)
				_logger.Log(level, "{Code}: {NugetMessage}", message.Code, message.Message);
			else
				_logger.Log(level, "{NugetMessage}", message.Message);
		}
	}

	/// <inheritdoc />
	public override Task LogAsync(ILogMessage message)
	{
		Log(message);
		return Task.CompletedTask;
	}
}