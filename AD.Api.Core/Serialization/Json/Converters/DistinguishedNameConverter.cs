using AD.Api.Core.Ldap;
using System.Globalization;
using System.Text;

namespace AD.Api.Core.Serialization.Json.Converters;

public sealed class DistinguishedNameConverter : JsonConverter<DistinguishedName>
{
	private const int MAX_STACKALLOC = 256;

	public override DistinguishedName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.String:
				return ParseFromSpan(ref reader);

			case JsonTokenType.Null:
				return DistinguishedName.Empty;

			default:
				throw new JsonException("Invalid JSON token type.", new FormatException("Distinguished names must be strings."));
		}
	}

	private static void HandleHexEscape(ref SpanStringBuilder builder, scoped Span<char> chars, ref int index)
	{
		if (index + 4 < chars.Length
			&&
			int.TryParse(chars.Slice(index + 1, 4), NumberStyles.HexNumber, null, out int unicodeHex))
		{
			builder.Append((char)unicodeHex);
			index += 4; // Skip the next 4 characters.
		}
		else
		{
			throw new JsonException("Invalid unicode escape sequence.");
		}
	}
	private static DistinguishedName ParseFromSpan(ref Utf8JsonReader reader)
	{
		int length = Encoding.UTF8.GetMaxCharCount(reader.ValueSpan.Length);

		using (var buffer = RentedBuffer.Rent<char>(
			length <= MAX_STACKALLOC
				? stackalloc char[length]
				: length))
		{
			int written = reader.CopyString(buffer.Span);

			ReadOnlySpan<char> value = buffer[..written];
			if (!DistinguishedName.TryCountNumberOfRelativeNames(value, out int count))
			{
				return DistinguishedName.Empty;
			}

			using (RentedBuffer<RelativeName> names = new(count))
			{
				if (!DistinguishedName.TrySplit(value, names.Span, out int namesWritten))
				{
					return DistinguishedName.Empty;
				}

				return new(names[..namesWritten]);
			}
		}
	}

	public override void Write(Utf8JsonWriter writer, DistinguishedName value, JsonSerializerOptions options)
	{
		int length = value.Length;
		if (length == 0)
		{
			writer.WriteStringValue(utf8Value: default);
			return;
		}

		using (var buffer = RentedBuffer.Rent<char>(
			length <= MAX_STACKALLOC
				? stackalloc char[length]
				: length))
		{
			int written = value.CopyTo(buffer.Span);
			writer.WriteStringValue(buffer[..written]);
		}
	}
}