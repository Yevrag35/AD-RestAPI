using AD.Api.Core.Serialization;
using System.Globalization;
using System.Text.Json;

namespace AD.Api.Mapping
{
    public static partial class AttributeSerialization
    {
        public static void WriteDateTimeOffset(Utf8JsonWriter writer, ref readonly SerializationContext context)
        {
            if (context.Value is long fileTimeValue)
            {
                WriteDateTimeFromFileTime(writer, in fileTimeValue, context.Options);
                return;
            }
            else if (context.Value is string strValue && DateTime.TryParse(strValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime result))
            {
                writer.WriteStringValue(new DateTimeOffset(result));
            }
            else
            {
                JsonSerializer.Serialize(writer, context.Value, context.Options);
            }
        }
        private static void WriteDateTimeFromFileTime(Utf8JsonWriter writer, in long fileTime, JsonSerializerOptions options)
        {
            DateTimeOffset offset;
            try
            {
                DateTime dt = DateTime.FromFileTime(fileTime);
                offset = new DateTimeOffset(dt.ToUniversalTime());
            }
            catch (ArgumentOutOfRangeException)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(offset);
        }
    }
}
