using AD.Api.Components;
using AD.Api.Core.Operations;
using AD.Api.Serialization.Converters;
using AD.Api.Serialization.Json;
using System.Globalization;

namespace AD.Api.Core.Serialization.Json.Converters.Ldap;

public abstract class EditOperationConverter<T, TValue> : ByPolicyJsonConverter<T>
	where T : EditOperationDictionary<TValue>
	where TValue : notnull
{
	protected EditOperationConverter(WorkingNamingPolicy policy) : base(policy)
	{
	}

	protected abstract T CreateCollection();
	protected abstract void Deserialize(ref Utf8JsonReader reader, T collection, JsonSerializerOptions options);
	protected abstract bool IsProperStartToken(JsonTokenType type);
	public sealed override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		T collection = this.CreateCollection();
		if (!this.IsProperStartToken(reader.TokenType))
		{
			return collection;
		}

		if (!reader.Read())
		{
			return collection;
		}

		this.Deserialize(ref reader, collection, options);
		return collection;
	}
	protected ObjEither<string, byte[], string[]> ReadValue(string key, ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.String => ReadGuidOrString(ref reader),
			JsonTokenType.True => LdapBoolean.TrueString,
			JsonTokenType.False => LdapBoolean.FalseString,
			JsonTokenType.Number => reader.TryGetInt64(out long longValue)
				? longValue.ToString() ?? string.Empty
				: reader.GetDecimal().ToString() ?? string.Empty,

			JsonTokenType.StartArray => this.ReadArray(key, ref reader, options),
			JsonTokenType.Null => throw new JsonException($"Unexpected null value in property '{key}' after deserializing LDAP operation."),
			_ => throw new JsonException($"Unexpected token type {reader.TokenType} for property '{key}'."),
		};
	}

	protected virtual string[] ReadArray(string key, ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		return JsonSerializer.Deserialize<string[]>(ref reader, options) ?? [];
	}

	private static ObjEither<string, byte[]> ReadGuidOrString(ref Utf8JsonReader reader)
	{
		return reader.TryGetGuid(out Guid guid)
			? guid.ToByteArray()
			: reader.GetString() ?? string.Empty;
	}

	protected static void SerializeMultipleValues(Utf8JsonWriter writer, IList list, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		for (int i = 0; i < list.Count; i++)
		{
			SerializeSingleValue(writer, list[i], options);
		}

		writer.WriteEndArray();
	}
	protected static void SerializeSingleValue(Utf8JsonWriter writer, object? attributeValue, JsonSerializerOptions options)
	{
		if (attributeValue is byte[] byteArray && TryNewGuid(byteArray, out Guid guid))
		{
			writer.WriteStringValue(guid);
			return;
		}
		else if (attributeValue is Uri url)
		{
			writer.WriteStringValue(url.ToString());
			return;
		}

		ReadOnlySpan<char> value = attributeValue?.ToString();
		if (TryGetNumber(value, out Either<long, decimal> number))
		{
			if (number.IsT1)
				writer.WriteNumberValue(number.AsT1);

			else
				writer.WriteNumberValue(number.AsT2);
		}
		else if (LdapBoolean.TryParseBool(value, out bool result) || bool.TryParse(value, out result))
		{
			writer.WriteBooleanValue(result);
		}
		else if (attributeValue is not null)
		{
			writer.WriteStringValue(value);
		}
		else
		{
			writer.WriteNullValue();
		}
	}
	private static bool TryGetNumber(ReadOnlySpan<char> span, out Either<long, decimal> number)
	{
		if (long.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longVal))
		{
			number = longVal;
			return true;
		}
		else if (decimal.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal decVal))
		{
			number = decVal;
			return true;
		}
		else
		{
			number = default;
			return false;
		}
	}
	private static bool TryNewGuid(ReadOnlySpan<byte> span, out Guid guid)
	{
		if (span.Length == 16)
		{
			try
			{
				guid = new(span);
				return true;
			}
			catch
			{
				Debug.Fail("not a guid");
			}
		}

		guid = Guid.Empty;
		return false;
	}
}