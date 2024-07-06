using AD.Api.Core.Serialization;
using AD.Api.Statics;
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
                WriteDateTimeFromFileTime(writer, in fileTimeValue);
                return;
            }
            else if (context.Value is string strValue && DateTime.TryParse(strValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime result))
            {
                writer.WriteStringValue(new DateTimeOffset(result));
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
        private static void WriteDateTimeFromFileTime(Utf8JsonWriter writer, in long fileTime)
        {
            if (LengthConstants.MaximumFileTimeValue < fileTime || long.IsNegative(fileTime))
            {
                writer.WriteNullValue();
                return;
            }

            DateTime dt = DateTime.FromFileTimeUtc(fileTime);
            writer.WriteStringValue(new DateTimeOffset(dt));
        }
    }
}
