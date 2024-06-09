using AD.Api.Ldap.Path;
using Newtonsoft.Json;

namespace AD.Api.Ldap.Converters
{
    public class PathValueJsonConverter : JsonConverter<PathValue>
    {
        public PathValueJsonConverter()
        {
        }

        public override PathValue? ReadJson(JsonReader reader, Type objectType, PathValue? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string? path = null;

            if (reader.TokenType == JsonToken.String)
            {
                path = reader.Value as string;
            }

            return PathValue.FromPath(path);
        }

        public override void WriteJson(JsonWriter writer, PathValue? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.GetValue() ?? string.Empty);
        }
    }
}
