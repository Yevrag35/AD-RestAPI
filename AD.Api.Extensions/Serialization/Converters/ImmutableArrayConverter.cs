using System.Collections.Immutable;

namespace AD.Api.Serialization.Converters;

public sealed class ImmutableArrayConverter<T> : JsonConverter<ImmutableArray<T>>
{
	public override ImmutableArray<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
		{
			throw new JsonException("Expected StartArray.");
		}

		using JsonDocument doc = JsonDocument.ParseValue(ref reader);
		JsonElement jsonArray = doc.RootElement;

		Debug.Assert(jsonArray.ValueKind == JsonValueKind.Array);

		int length = jsonArray.GetArrayLength();
		if (length == 0)
			return [];

		int index = 0;
		T[] array = new T[length];
		foreach (JsonElement item in jsonArray.EnumerateArray())
		{
			if (item.Deserialize<T>(options) is T value)
			{
				array[index++] = value;
			}
		}

		return index < length
			? [.. array.AsSpan(0, index)]
			: ImmutableCollectionsMarshal.AsImmutableArray(array);
	}

	public override void Write(Utf8JsonWriter writer, ImmutableArray<T> value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		if (value.IsDefaultOrEmpty)
		{
			writer.WriteEndArray();
			return;
		}

		foreach (T item in value.AsSpan())
		{
			JsonSerializer.Serialize(writer, item, options);
		}

		writer.WriteEndArray();
	}
}