using AD.Api.Ldap.Properties;
using Newtonsoft.Json;

namespace AD.Api.Ldap.Converters
{
    public class ProxyAddressConverter : JsonConverter<ProxyAddressCollection>
    {
        public ProxyAddressConverter()
        {
        }

        public override bool CanRead => false;
        public override ProxyAddressCollection? ReadJson(JsonReader reader, Type objectType, ProxyAddressCollection? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, ProxyAddressCollection? value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if (value?.Count > 0)
            {
                value.ForEach(pa =>
                {
                    serializer.Serialize(writer, pa.GetValue());
                });
            }

            writer.WriteEndArray();
        }
    }
}
