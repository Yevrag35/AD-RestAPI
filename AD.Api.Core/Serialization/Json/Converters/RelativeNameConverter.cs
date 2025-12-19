using AD.Api.Core.Ldap;
using System.Text;

namespace AD.Api.Core.Serialization.Json.Converters;

public sealed class RelativeNameConverter : JsonConverter<RelativeName>
{
	public override RelativeName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.String:
				return ParseFromSpan(ref reader);

			case JsonTokenType.Null:
			default:
				return RelativeName.Empty;
		}
	}

	private static RelativeName ParseFromSpan(ref Utf8JsonReader reader)
	{
		const int MAX_STACKALLOC_SIZE = 256;
		int length = Encoding.UTF8.GetMaxCharCount(reader.ValueSpan.Length);

		using (var buffer = RentedBuffer.Rent<char>(
			length <= MAX_STACKALLOC_SIZE
				? stackalloc char[length]
				: length))
		{
			int written = Encoding.UTF8.GetChars(reader.ValueSpan, buffer.Span);
			RelativeName result = RelativeName.TryParse(buffer[..written], RelativeNameType.CommonName, out RelativeName rn)
				? rn
				: RelativeName.Empty;

			return result;
		}
	}

	public override void Write(Utf8JsonWriter writer, RelativeName value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Value);
	}
}
