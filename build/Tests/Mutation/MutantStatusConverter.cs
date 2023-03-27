using System;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class MutantStatusConverter : JsonConverter<MutantStatus>
{
	public override bool CanConvert(Type t) => t == typeof(MutantStatus);

	public override MutantStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		switch (value)
		{
			case "CompileError":
				return MutantStatus.CompileError;
			case "Ignored":
				return MutantStatus.Ignored;
			case "Killed":
				return MutantStatus.Killed;
			case "NoCoverage":
				return MutantStatus.NoCoverage;
			case "RuntimeError":
				return MutantStatus.RuntimeError;
			case "Survived":
				return MutantStatus.Survived;
			case "Timeout":
				return MutantStatus.Timeout;
		}
		throw new Exception("Cannot unmarshal type MutantStatus");
	}

	public override void Write(Utf8JsonWriter writer, MutantStatus value, JsonSerializerOptions options)
	{
		switch (value)
		{
			case MutantStatus.CompileError:
				JsonSerializer.Serialize(writer, "CompileError", options);
				return;
			case MutantStatus.Ignored:
				JsonSerializer.Serialize(writer, "Ignored", options);
				return;
			case MutantStatus.Killed:
				JsonSerializer.Serialize(writer, "Killed", options);
				return;
			case MutantStatus.NoCoverage:
				JsonSerializer.Serialize(writer, "NoCoverage", options);
				return;
			case MutantStatus.RuntimeError:
				JsonSerializer.Serialize(writer, "RuntimeError", options);
				return;
			case MutantStatus.Survived:
				JsonSerializer.Serialize(writer, "Survived", options);
				return;
			case MutantStatus.Timeout:
				JsonSerializer.Serialize(writer, "Timeout", options);
				return;
		}
		throw new Exception("Cannot marshal type MutantStatus");
	}

	public static readonly MutantStatusConverter Singleton = new MutantStatusConverter();
}