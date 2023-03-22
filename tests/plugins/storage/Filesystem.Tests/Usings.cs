using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using VespionSoftworks.Athenaeum.TestUtilities.Logger;
using Xunit;
using Xunit.Abstractions;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests;

public static class Helpers
{
	public static (StoragePlugin, MockFileSystem, InMemoryLogger) CreatePlugin(ITestOutputHelper? output = null, MockFileSystem? fs = null)
	{
		var provider = new InMemoryLoggerProvider();
		var lb = new LoggerFactory();
		lb.AddProvider(provider);
		
		if (output != null)
		{
			lb.AddXUnit(output);
		}

		fs ??= new MockFileSystem();
		var plugin = new StoragePlugin(fs.DirectoryInfo.New("/"), lb.CreateLogger<StoragePlugin>());
		
		var logger = Assert.Single(provider.Loggers.Values);
		Assert.Equal("VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.StoragePlugin", logger.Name);
		return (plugin, fs, logger);
	}
}