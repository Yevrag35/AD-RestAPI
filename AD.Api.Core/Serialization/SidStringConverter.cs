using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization
{
    public sealed class SidStringConverter : JsonConverter<SecurityIdentifier>
    {
        public override SecurityIdentifier? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, SecurityIdentifier value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

