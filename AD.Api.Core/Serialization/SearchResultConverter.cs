using AD.Api.Core.Ldap;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class SearchResultConverter : JsonConverter<ResultEntry>
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
}

