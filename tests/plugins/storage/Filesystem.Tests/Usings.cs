using System.IO.Abstractions.TestingHelpers;
using InMemLogger;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests;

public static class Helpers
{
	public static (StoragePlugin, MockFileSystem, InMemoryLogger) CreatePlugin(ITestOutputHelper? output = null, MockFileSystem? fs = null)
	{
		var logger = new InMemoryLogger();

		var lb = new LoggerFactory();
		lb.AddProvider(new InMemLoggerProvider(logger));

		if (output != null)
		{
			lb.AddXUnit(output);
		}
		
		fs ??= new MockFileSystem();
		return (new StoragePlugin(fs.DirectoryInfo.New("/"), lb.CreateLogger<StoragePlugin>()), fs, logger);
	}
}