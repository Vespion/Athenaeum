using FluentAssertions;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using VespionSoftworks.Athenaeum.TestUtilities.Logger;
using LogLevel = NuGet.Common.LogLevel;
using LogMessage = NuGet.Common.LogMessage;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Tests;

public class NugetLoggerTests
{
	public static TheoryData<string, LogLevel, NuGetLogCode?> LogData
	{
		get
		{
			var data = new TheoryData<string, LogLevel, NuGetLogCode?>();

			var allCodeSamples = Enum.GetValues<NuGetLogCode>();
			var codeSamplesChunked = allCodeSamples.Chunk(5);
			var codeSamples = codeSamplesChunked
				.Select(x => x.Length == 5 ? x.Skip(2).First() : x[0])
				.Select(x => new NuGetLogCode?(x))
				.ToArray();

			var codeList = new NuGetLogCode?[codeSamples.Length + 1];
			codeList[0] = null;
			Array.Copy(codeSamples, 0, codeList, 1, codeSamples.Length);
			
			foreach (var level in Enum.GetValues<LogLevel>())
			{
				foreach (var code in codeList)
				{
					data.Add("Ta Da! This is a test log message", level, code);
				}
			}
			
			return data;
		}
	}

	private static (NugetLogger, InMemoryLogger, ILogMessage) BuildObjects(string msg, LogLevel level, NuGetLogCode? code)
	{
		var logMessage = code.HasValue ? new LogMessage(level, msg, code.Value) : new LogMessage(level, msg);

		var provider = new InMemoryLoggerProvider();
		var lb = new LoggerFactory();
		lb.AddProvider(provider);
		
		var nugetLogger = new NugetLogger(lb.CreateLogger<NugetLogger>());

		var logger = Assert.Single(provider.Loggers.Values);
		
		Assert.Equal("VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.NugetLogger", logger.Name);
		
		return (nugetLogger, logger, logMessage);
	}
	
	private static void RunAssertions(InMemoryLogger logger, string msg, LogLevel level, NuGetLogCode? code)
	{
		Microsoft.Extensions.Logging.LogLevel Convert(LogLevel l)
		{
			return l switch
			{
				LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
				LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Trace,
				LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
				LogLevel.Minimal => Microsoft.Extensions.Logging.LogLevel.Trace,
				LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
				LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
				_ => throw new ArgumentOutOfRangeException(nameof(l), l, null)
			};
		}
		
		var log = Assert.Single(logger.RecordedLogs);
		
		Assert.Equal(Convert(level), log.Level);
		
		bool IsCodeScoped(KeyValuePair<string, object> x)
		{
			if (x.Key != "nuget_code")
			{
				return false;
			}
				
			var parseResult = Enum.TryParse<NuGetLogCode>(x.Value.ToString(), out var parsed);
			if (!parseResult)
			{
				return false;
			}

			return parsed == (code ?? NuGetLogCode.Undefined);
		}
		log.Scopes.Should().HaveCount(1);
		IsCodeScoped(log.Scopes.First()).Should().BeTrue();
		
		// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
		if (code.HasValue)
		{
			Assert.Equal($"{code}: {msg}", log.Message);
		}
		else
		{
			Assert.Equal(msg, log.Message);
		}
	}
	
	[Theory]
	[MemberData(nameof(LogData))]
	public void LogsMessage(string msg, LogLevel level, NuGetLogCode? code)
	{
		var (nugetLogger, logger, logMessage) = BuildObjects(msg, level, code);
		
		nugetLogger.Log(logMessage);
		
		RunAssertions(logger, msg, level, code);
	}
	
	[Theory]
	[MemberData(nameof(LogData))]
	public async Task LogsMessageAsync(string msg, LogLevel level, NuGetLogCode? code)
	{
		var (nugetLogger, logger, logMessage) = BuildObjects(msg, level, code);
		
		await nugetLogger.LogAsync(logMessage);
		
		RunAssertions(logger, msg, level, code);
	}
}