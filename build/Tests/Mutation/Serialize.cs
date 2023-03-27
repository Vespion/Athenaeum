using System.Text.Json;

public static class Serialize
{
	public static string ToJson(this MutationReport self) => JsonSerializer.Serialize(self, Converter.Settings);
	public static string ToJson(this MutantResult self) => JsonSerializer.Serialize(self, Converter.Settings);
	public static MutationReport? FromJson(string self) => JsonSerializer.Deserialize<MutationReport>(self, Converter.Settings);
}