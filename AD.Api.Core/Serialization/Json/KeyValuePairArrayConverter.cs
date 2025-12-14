using AD.Api.Core.Schema;
using AD.Api.Serialization.Json;

namespace AD.Api.Core.Serialization.Json;

public sealed class KeyValuePairArrayConverter<T> : JsonConverter<KeyValuePair<string, T>[]>
{
	private static readonly Type _typeDef = typeof(KeyValuePair<,>);
	private static readonly Type _keyType = SchemaProperty.StringType;
	private readonly Type _type;

	public bool IsOrdered { get; init; }

	public KeyValuePairArrayConverter()
	{
		_type = typeof(KeyValuePair<string, T>[]);
	}

	public override bool CanConvert(Type typeToConvert)
	{
		bool equals = _type.Equals(typeToConvert);
		return equals;
	}

	public override KeyValuePair<string, T>[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotSupportedException();
	}

	public override void Write(Utf8JsonWriter writer, KeyValuePair<string, T>[] value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		if (value.Length <= 0)
		{
			writer.WriteEndObject();
			return;
		}

		if (this.IsOrdered)
		{
			Array.Sort(value, (x, y) => string.CompareOrdinal(x.Key, y.Key));
		}

		WorkingNamingPolicy policy = new(options);

		for (int i = 0; i < value.Length; i++)
		{
			KeyValuePair<string, T> pair = value[i];
			policy.WritePropertyName(writer, pair.Key);
			JsonSerializer.Serialize(writer, pair.Value, options);
		}

		writer.WriteEndObject();
	}
}
