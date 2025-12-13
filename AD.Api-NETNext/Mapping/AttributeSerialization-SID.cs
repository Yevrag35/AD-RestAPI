using AD.Api.Core.Security;
using AD.Api.Core.Serialization;
using System.Text.Json;

namespace AD.Api.Mapping;

public static partial class AttributeSerialization
{
	public static void WriteObjectSID(Utf8JsonWriter writer, ref readonly SerializationContext context)
	{
		if (context.Value is not byte[] sidBytes)
		{
			WriteNonByteSid(writer, context.Value, context.Options);
			return;
		}

		WriteSid(writer, sidBytes);
	}

	private static void WriteSid(Utf8JsonWriter writer, ReadOnlySpan<byte> sidBytes)
	{
		Span<char> chars = stackalloc char[SidString.MaxSidStringLength];
		int written = SidString.FormatSpan(chars, sidBytes);

		writer.WriteStringValue(chars.Slice(0, written));
	}
	private static void WriteNonByteSid(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
		}
		else if (value is string sidString)
		{
			writer.WriteStringValue(sidString);
		}
		else
		{
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
