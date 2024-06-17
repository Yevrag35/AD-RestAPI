using AD.Api.Core.Ldap.Results;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class ResultEntryConverter : JsonConverter<ResultEntry>
    {
        private readonly PropertyConverter _converter;

        public ResultEntryConverter(PropertyConverter converter)
        {
            _converter = converter;
        }

        public override ResultEntry? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, ResultEntry value, JsonSerializerOptions options)
        {
            _converter.WriteTo(writer, value, options);
        }
    }

    [SupportedOSPlatform("WINDOWS")]
    public sealed class ResultEntryCollectionConverter : JsonConverter<ResultEntryCollection>
    {
        private readonly PropertyConverter _converter;

        public ResultEntryCollectionConverter(PropertyConverter converter)
        {
            _converter = converter;
        }

        public override ResultEntryCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, ResultEntryCollection value, JsonSerializerOptions options)
        {
            _converter.WriteTo(writer, value, options);
        }
    }
}

