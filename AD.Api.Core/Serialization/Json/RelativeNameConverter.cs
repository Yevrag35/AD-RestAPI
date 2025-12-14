using AD.Api.Core.Ldap;
using System.Text;

namespace AD.Api.Core.Serialization.Json;

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
		int length = Encoding.UTF8.GetMaxCharCount(reader.ValueSpan.Length);
		bool isRented = false;
		char[]? array = null;

		Span<char> span = length <= 256
			? stackalloc char[length]
			: SpanExtensions.RentArray(in length, ref isRented, ref array);

		int written = Encoding.UTF8.GetChars(reader.ValueSpan, span);
		RelativeName result = RelativeName.TryParse(span.Slice(0, written), RelativeNameType.CommonName, out RelativeName rn)
			? rn
			: RelativeName.Empty;

		if (isRented)
		{
			ArrayPool<char>.Shared.Return(array!);
		}

		return result;
	}

	public override void Write(Utf8JsonWriter writer, RelativeName value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Value);
	}
}
