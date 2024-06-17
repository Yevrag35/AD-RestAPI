using AD.Api.Core.Serialization;
using System.Globalization;
using System.Text.Json;

namespace AD.Api.Mapping
{
    public static partial class AttributeSerialization
    {
        internal const long MinFileTimeValue = 0L; // January 1, 1601
        internal const long MaxFileTimeValue = 2650467743999999999L; // DateTime.MaxValue in FILETIME

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
            if (MaxFileTimeValue < fileTime || long.IsNegative(fileTime))
            {
                writer.WriteNullValue();
                return;
            }

            DateTime dt = DateTime.FromFileTimeUtc(fileTime);
            writer.WriteStringValue(new DateTimeOffset(dt));
        }
    }
}
