using AD.Api.Core.Ldap.Results;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class ResultEntryConverter : JsonConverter<ResultEntry>
    {
        public override ResultEntry? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, ResultEntry value, JsonSerializerOptions options)
        {
            value.WriteTo(writer, options);
        }
    }

    [SupportedOSPlatform("WINDOWS")]
    public sealed class ResultEntryCollectionConverter : JsonConverter<ResultEntryCollection>
    {
        public override ResultEntryCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, ResultEntryCollection value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            if (value.Count == 0)
            {
                writer.WriteEndArray();
                return;
            }

            foreach (ResultEntry entry in value)
            {
                entry.WriteTo(writer, options);
            }

            writer.WriteEndArray();
        }
    }
}

