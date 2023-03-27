using System.Text.Json;

internal static class Converter
{
	public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
	{
		Converters =
		{
			MutantStatusConverter.Singleton,
			new DateOnlyConverter(),
			new TimeOnlyConverter(),
			IsoDateTimeOffsetConverter.Singleton
		},
	};
}