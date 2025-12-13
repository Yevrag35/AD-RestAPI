using AD.Api.Components;
using System.Text.Json;

namespace AD.Api.Serialization.Json;

public static class JsonWriterExtensions
{
	public static void WriteLdapBoolValue(this Utf8JsonWriter writer, ReadOnlySpan<char> boolChars)
	{
		if (LdapBoolean.TryParseBool(boolChars, out bool result))
		{
			writer.WriteBooleanValue(result);
		}
		else
		{
			writer.WriteNullValue();
		}
	}

	public static void WriteEmptyArray(this Utf8JsonWriter writer)
	{
		writer.WriteStartArray();
		writer.WriteEndArray();
	}
	public static void WriteEmptyObject(this Utf8JsonWriter writer)
	{
		writer.WriteStartObject();
		writer.WriteEndObject();
	}

	public static Utf8JsonWriter WriteFileTime(this Utf8JsonWriter writer, in OneOf<DateTimeOffset, long> fileTime)
	{
		return fileTime.Match(writer,
			f0: (w, offset) =>
			{
				w.WriteStringValue(offset);
				return w;
			},
			f1: (w, longVal) =>
			{
				w.WriteNumberValue(longVal);
				return w;
			});
	}
	public static Utf8JsonWriter WriteNumberValue(this Utf8JsonWriter writer, in OneOf<long, decimal> number)
	{
		return number.Match(writer,
			f0: (w, longVal) =>
			{
				w.WriteNumberValue(longVal);
				return w;
			},
			f1: (w, decVal) =>
			{
				w.WriteNumberValue(decVal);
				return w;
			});
	}
}
