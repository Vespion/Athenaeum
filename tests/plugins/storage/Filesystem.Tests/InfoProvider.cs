using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Xunit;

namespace VespionSoftworks.Athenaeum.Plugins.Storage.Filesystem.Tests;

public class TestLocalizer: IStringLocalizer<StoragePlugin>
{
	private readonly IDictionary<string, string> _strings = new Dictionary<string, string>
	{
		{ nameof(InfoProvider.Name), "Filesystem Storage" },
		{ nameof(InfoProvider.Description), "A plugin that allows you to store your data in the filesystem." }
	};

	/// <inheritdoc />
	public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
	{
		throw new UnreachableException();
	}

	/// <inheritdoc />
	public LocalizedString this[string name] => new (name, _strings[name]);

	/// <inheritdoc />
	public LocalizedString this[string name, params object[] arguments] => throw new UnreachableException();
}

public class InfoProviderTests
{
	[Fact]
	public void ProvidesExpectedStaticStrings()
	{
		var provider = new InfoProvider(null!);
		
		provider.Author.Should().Be("Vespion Softworks");
		provider.MinimumHostVersion.Should().Be("1.0.0");
		provider.MaximumHostVersion.Should().BeNull();
	}

	[Fact]
	public void ProvidesLocalisedStrings()
	{
		var provider = new InfoProvider(new TestLocalizer());
		
		provider.Name.Should().Be("Filesystem Storage");
		provider.Description.Should().Be("A plugin that allows you to store your data in the filesystem.");
	}
}