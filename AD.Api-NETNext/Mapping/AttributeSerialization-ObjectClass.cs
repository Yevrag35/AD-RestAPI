using AD.Api.Core.Serialization;

namespace AD.Api.Mapping;

public static partial class AttributeSerialization
{
	public static void WriteObjectClassSimple(Utf8JsonWriter writer, ref readonly SerializationContext context)
	{
		if (context.Value is string[] array)
		{
			writer.WriteStringValue(array[^1]);
		}
		else if (context.Value is string str)
		{
			writer.WriteStringValue(str);
		}
		else if (context.Value is not null)
		{
			JsonSerializer.Serialize(writer, context.Value, context.Value.GetType(), context.Options);
		}
		else
		{
			writer.WriteNullValue();
		}
	}
}
