using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Xunit;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests;

public class Bootstrapper
{
	[Fact]
	public void LocalisationPathCorrect()
	{
		var serviceCollection = new ServiceCollection();

		var boot = new Plugins.Storage.Filesystem.Bootstrapper();
		
		boot.ConfigureServices(serviceCollection);
		
		var options = serviceCollection.BuildServiceProvider().GetRequiredService<IOptions<LocalizationOptions>>();

		options.Value.ResourcesPath.Should().Be("Resources");
	}
	
	[Fact]
	public void AddsExpectedService()
	{
		var serviceCollection = new ServiceCollection();

		var boot = new Plugins.Storage.Filesystem.Bootstrapper();
		
		boot.ConfigureServices(serviceCollection);
		
		serviceCollection.Should()
			.ContainSingle(s => 
				s.ServiceType == typeof(IFileSystem) &&
				s.ImplementationType == typeof(FileSystem) &&
				s.Lifetime == ServiceLifetime.Singleton
			);
	}
	
	[Fact]
	public void DoesNotErrorIfServiceExists()
	{
		var serviceCollection = new ServiceCollection();

		serviceCollection.AddSingleton<IFileSystem, MockFileSystem>();
		
		var boot = new Plugins.Storage.Filesystem.Bootstrapper();
		
		boot.ConfigureServices(serviceCollection);
		
		serviceCollection.Should()
			.ContainSingle(s => 
				s.ServiceType == typeof(IFileSystem) &&
				s.ImplementationType == typeof(MockFileSystem) &&
				s.Lifetime == ServiceLifetime.Singleton
			);
	}
}