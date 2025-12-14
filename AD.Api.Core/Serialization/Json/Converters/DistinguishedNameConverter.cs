using AD.Api.Core.Ldap;
using AD.Api.Statics;
using System.Globalization;
using System.Text;

using ColEx = AD.Api.Collections.CollectionExtensions;

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
		bool isRented = false;
		char[]? array = null;

		scoped Span<char> span = length <= 256
			? stackalloc char[length]
			: ColEx.RentArray(in length, ref isRented, ref array);

		scoped ReadOnlySpan<char> value;
		if (reader.ValueIsEscaped)
		{
			value = UnescapeValue(reader.ValueSpan, span, in length);
		}
		else
		{
			int written = Encoding.UTF8.GetChars(reader.ValueSpan, span);
			value = span.Slice(0, written);
		}

		if (!DistinguishedName.TryCountNumberOfRelativeNames(value, out int count))
		{
			return DistinguishedName.Empty;
		}

		RelativeName[] buffer = ArrayPool<RelativeName>.Shared.Rent(count);
		try
		{
			if (!DistinguishedName.TrySplit(span, buffer.AsSpan(0, count), out int namesWritten))
			{
				ArrayPool<RelativeName>.Shared.Return(buffer);
				return DistinguishedName.Empty;
			}

			return new(buffer.AsSpan(0, namesWritten));
		}
		finally
		{
			ArrayHelper.ReturnToPool(buffer);
		}
	}
	private static Span<char> UnescapeValue(ReadOnlySpan<byte> value, Span<char> buffer, in int length)
	{
		SpanStringBuilder builder = new(buffer);
		bool escaping = false;

		bool isRented = false;
		char[]? array = null;

		Span<char> chars = length <= MAX_STACKALLOC
			? stackalloc char[length]
			: ColEx.RentArray(in length, ref isRented, ref array);

		int written = Encoding.UTF8.GetChars(value, chars);
		chars = chars.Slice(0, written);

		for (int i = 0; i < written; i++)
		{
			ref char c = ref chars[i];
			if (escaping)
			{
				switch (c)
				{
					case '"':
					case '\\':
					case '/':
						builder.Append(c);
						break;

					case 'b':
						builder.Append('\b');
						break;

					case 'f':
						builder.Append('\f');
						break;

					case 'n':
						builder.Append('\n');
						break;

					case 'r':
						builder.Append('\r');
						break;

					case 't':
						builder.Append('\t');
						break;

					case 'u':
						HandleHexEscape(ref builder, chars, ref i);
						break;

					default:
						throw new JsonException("Invalid escape sequence.");
				}

				escaping = false;
			}
			else if (CharConstants.BACKSLASH == c)
			{
				escaping = true;
			}
			else
			{
				builder.Append(c);
			}
		}

		if (escaping)
		{
			throw new JsonException("Invalid escape sequence at the end of the string.");
		}

		if (isRented)
		{
			ArrayPool<char>.Shared.Return(array!);
		}

		return builder.
	}

	public override void Write(Utf8JsonWriter writer, DistinguishedName value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}