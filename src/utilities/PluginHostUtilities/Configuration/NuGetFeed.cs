using System.ComponentModel.DataAnnotations;

namespace VespionSoftworks.Athenaeum.Utilities.PluginHostUtilities.Configuration;

public class NuGetFeed
{
	[Required]
	public string Name { get; set; }
	[Required, Url]
	public string Url { get; set; }
}