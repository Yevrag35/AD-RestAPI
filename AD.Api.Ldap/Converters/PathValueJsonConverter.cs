using AD.Api.Ldap.Path;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Converters
{
    public class PathValueJsonConverter : JsonConverter<PathValue>
    {
        public override PathValue? ReadJson(JsonReader reader, Type objectType, PathValue? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string? path = null;

            if (reader.TokenType == JsonToken.String)
            {
                path = reader.ReadAsString();
            }

            return PathValue.FromPath(path);
        }

        public override void WriteJson(JsonWriter writer, PathValue? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.GetValue() ?? string.Empty);
        }
    }
}
