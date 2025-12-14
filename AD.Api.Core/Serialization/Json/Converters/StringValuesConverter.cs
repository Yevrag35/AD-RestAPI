using Microsoft.Extensions.Primitives;

namespace AD.Api.Core.Serialization.Json.Converters;

public sealed class StringValuesConverter : JsonConverter<StringValues>
{
	public override StringValues Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.String:
				return reader.GetString();

			case JsonTokenType.Number:
				return reader.TryGetInt64(out long value) ? value.ToString() : reader.GetDouble().ToString();

			case JsonTokenType.Null:
			case JsonTokenType.None:
			case JsonTokenType.PropertyName:
			case JsonTokenType.EndArray:
			case JsonTokenType.EndObject:
			case JsonTokenType.Comment:
				return StringValues.Empty;

			case JsonTokenType.StartObject:
				throw new JsonException("Cannot convert JSON object to StringValues.");

			case JsonTokenType.StartArray:
				return JsonSerializer.Deserialize<string[]>(ref reader, options);

			case JsonTokenType.True:
				return bool.TrueString;

			case JsonTokenType.False:
				return bool.FalseString;

			default:
				throw new JsonException("Unexpected token type.");
		}
	}

	public override void Write(Utf8JsonWriter writer, StringValues value, JsonSerializerOptions options)
	{
		if (StringValues.IsNullOrEmpty(value))
		{
			writer.WriteStringValue(ReadOnlySpan<char>.Empty);
			return;
		}
		else if (value.Count == 1)
		{
			writer.WriteStringValue(value[0]);
			return;
		}

		writer.WriteStartArray();
		foreach (string? item in value)
		{
			writer.WriteStringValue(item);
		}

		writer.WriteEndArray();
	}
}

