using AD.Api.Core.Security;
using System.Text;

namespace AD.Api.Core.Serialization.Json.Converters;

public sealed class SidStringConverter : JsonConverter<SidString>
{
	public override SidString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
		{
			return null;
		}

		int length = Encoding.UTF8.GetMaxCharCount(reader.ValueSpan.Length);
		if (!SidString.IsLengthInRange<char>(in length))
		{
			//TODO: Log not SID message.
			return null;
		}

		Span<char> chars = stackalloc char[length];
		int written = Encoding.UTF8.GetChars(reader.ValueSpan, chars);
		if (!SidString.TryParse(chars.Slice(0, written), out SidString? sid))
		{
			//TODO: Log not SID message.
			return null;
		}
		else
		{
			return sid;
		}
	}

	public override void Write(Utf8JsonWriter writer, SidString value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Value);
	}
}

