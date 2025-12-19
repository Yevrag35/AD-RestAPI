using AD.Api.Collections.Intrinsics;
using AD.Api.Extensions.Strings;
using AD.Api.Serialization.Json;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace AD.Api.Core.Serialization.Json.Converters;

public abstract class StringValuesConverter : JsonConverter<StringValues>
{
	protected const int MAX_STACKALLOC_SIZE = 256;
	private static readonly StringValues s_false = bool.FalseString;
	private static readonly StringValues s_true = bool.TrueString;

	public static readonly StringValuesConverter AsArray = new ArrayConverter();

	/// <summary>
	/// Gets the serialization mode used by the current instance.
	/// </summary>
	/// <remarks>A value of 1 indicates that serialization is performed as a string; a value of 0 indicates an
	/// alternative serialization mode. Use this property to determine how data will be serialized when this converter executes.
	/// <para>
	/// This property is immutable and is set during the construction of the converter instance.
	/// </para></remarks>
	public SerializationMode Mode { get; }

	private protected StringValuesConverter(bool serializeAsString)
	{
		this.Mode = serializeAsString ? SerializationMode.AsString : SerializationMode.AsArray;
	}

	public sealed override bool CanConvert(Type typeToConvert)
	{
		if (Nullable.GetUnderlyingType(typeToConvert) is Type underlyingType)
		{
			typeToConvert = underlyingType;
		}

		return typeof(StringValues).Equals(typeToConvert);
	}

	public sealed override StringValues Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.StartArray => ReadAsArray(ref reader),
			JsonTokenType.Comment => this.SkipToValue(ref reader, typeToConvert, options),
			JsonTokenType.String => this.ReadAsString(ref reader, options),
			JsonTokenType.Number => reader.GetDouble().ToString(),
			JsonTokenType.True => s_true,
			JsonTokenType.False => s_false,
			JsonTokenType.Null => StringValues.Empty,
			_ => throw new JsonException($"Invalid JSON token for StringValues - Expected 'String' -or- 'StartArray' but got '{reader.TokenType}'."),
		};
	}

	private StringValues SkipToValue(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		while (reader.TokenType == JsonTokenType.Comment && reader.TrySkip())
		{ }

		return this.Read(ref reader, typeToConvert, options);
	}

	private static StringValues ReadAsArray(ref Utf8JsonReader reader)
	{
		using JsonDocument doc = JsonDocument.ParseValue(ref reader);
		JsonElement arrayElement = doc.RootElement;

		int count = arrayElement.GetArrayLength();
		if (count == 0)
			return StringValues.Empty;

		string?[] array = new string[count];

		int i = 0;
		foreach (JsonElement value in arrayElement.EnumerateArray())
		{
			string? item = value.ValueKind switch
			{
				JsonValueKind.String => value.GetString(),
				JsonValueKind.Null => null,
				JsonValueKind.True => bool.TrueString,
				JsonValueKind.False => bool.FalseString,
				JsonValueKind.Number => value.GetDouble().ToString(),
				_ => throw new JsonException("Invalid value type in StringValues array."),
			};

			array[i++] = item;
		}

		return array;
	}
	protected StringValues ReadAsString(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		int maxLength = Encoding.UTF8.GetMaxCharCount(reader.ValueSpan.Length);
		using (var buffer = RentedBuffer.Rent<char>(
			maxLength <= MAX_STACKALLOC_SIZE
				? stackalloc char[maxLength]
				: maxLength))
		{
			int written = reader.CopyString(buffer.Span);
			return written > 0
				? new string(buffer[..written])
				: StringValues.Empty;
		}
	}

	public sealed override void Write(Utf8JsonWriter writer, StringValues value, JsonSerializerOptions options)
	{
		object? rawValue = StringValuesMarshal.GetRawValue(value);

		switch (rawValue)
		{
			case null:
			case string emptyString when "".Equals(emptyString):
				this.WriteEmptyValue(writer, options);
				break;

			case string rawString:
				this.WriteValues(writer, options, rawString);
				break;

			case string[] strArray:
				this.WriteValues(writer, options, strArray);
				break;

			default:
				goto case null;
		}
	}

	private void WriteValues(Utf8JsonWriter writer, JsonSerializerOptions options, params ReadOnlySpan<string?> values)
	{
		if (values.IsEmpty)
		{
			this.WriteEmptyValue(writer, options);
		}
		else
		{
			this.WriteValues(writer, values, options);
		}
	}
	/// <summary>
	/// Writes the specified string values to the provided JSON writer using the given serialization options.
	/// </summary>
	/// <param name="writer">The JSON writer to which the values will be written. Must not be null.</param>
	/// <param name="values">A read-only span of string values to write. Elements may be null to represent JSON null values.</param>
	/// <param name="options">The options to use for JSON serialization. Must not be null.</param>
	protected abstract void WriteValues(Utf8JsonWriter writer, ReadOnlySpan<string?> values, JsonSerializerOptions options);
	/// <summary>
	/// Writes an empty JSON value to the specified writer using the provided serialization options.
	/// </summary>
	/// <param name="writer">The <see cref="Utf8JsonWriter"/> to which the empty value will be written. Must not be null.</param>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> that influence how the empty value is written. Must not be null.</param>
	protected abstract void WriteEmptyValue(Utf8JsonWriter writer, JsonSerializerOptions options);

	/// <summary>
	/// Specifies the format to use when serializing data, either as an array or as a string.
	/// </summary>
	public enum SerializationMode
	{
		/// <summary>
		/// Serializes the values as a JSON array.
		/// </summary>
		AsArray,
		/// <summary>
		/// Serializes the values as a single JSON string, with individual values concatenated and separated by commas.
		/// </summary>
		AsString
	}

	private sealed class ArrayConverter : StringValuesConverter
	{
		internal ArrayConverter() : base(false) { }

		protected override void WriteEmptyValue(Utf8JsonWriter writer, JsonSerializerOptions options)
		{
			writer.WriteEmptyArray();
		}
		protected override void WriteValues(Utf8JsonWriter writer, ReadOnlySpan<string?> values, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (string? s in values)
			{
				writer.WriteStringValue(s);
			}

			writer.WriteEndArray();
		}
	}
}
public sealed class StringValuesAsStringConverter : StringValuesConverter
{
	public StringValuesAsStringConverter() : base(true) { }

	protected override void WriteEmptyValue(Utf8JsonWriter writer, JsonSerializerOptions options)
	{
		writer.WriteStringValue(ReadOnlySpan<byte>.Empty);
	}
	protected override void WriteValues(Utf8JsonWriter writer, ReadOnlySpan<string?> values, JsonSerializerOptions options)
	{
		ReadOnlySpan<byte> separator = ", "u8;
		int length = Encoding.UTF8.GetMaxByteCount(values.GetConcatenatedLength(separatorLength: separator.Length));

		using (var buffer = RentedBuffer.Rent<byte>(
			length <= MAX_STACKALLOC_SIZE
				? stackalloc byte[length]
				: length))
		{
			int written = values.JoinBy(buffer.Span, separator);
			writer.WriteStringValue(buffer[..written]);
		}
	}
}
