using AD.Api.Core.Operations;
using AD.Api.Serialization.Json;
using System.Text.Json;

namespace AD.Api.Core.Serialization.Json.Ldap;

public sealed class OneEditOperationConverter<T> : EditOperationConverter<T, DirectoryAttributeModification>
    where T : EditOperationDictionary<DirectoryAttributeModification>, IAppendableSingleOperation, new()
{
    protected override T CreateCollection()
    {
        return new();
    }
    protected override void Deserialize(ref Utf8JsonReader reader, T collection, JsonSerializerOptions options)
    {
        while (reader.TokenType == JsonTokenType.PropertyName)
        {
            string key = reader.GetString() ?? throw new JsonException("Property keys cannot be null or empty");
            if (!reader.Read())
            {
                throw new JsonException("Unexpected end of JSON object.");
            }

            var oneOf = this.ReadValue(key, ref reader, options);
            collection.Add(key, oneOf);
            if (!reader.Read())
            {
                break;
            }
        }
    }
    protected override bool IsProperStartToken(JsonTokenType type)
    {
        return JsonTokenType.StartObject == type;
    }
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (DirectoryAttributeModification mod in value)
        {
            writer.WritePropertyName(mod.Name);
            switch (mod.Count)
            {
                case 0:
                    goto default;

                case 1:
                    SerializeSingleValue(writer, mod[0], options);
                    break;

                case > 1:
                    SerializeMultipleValues(writer, mod, options);
                    break;

                default:
                    writer.WriteEmptyArray();
                    break;
            }
        }

        writer.WriteEndObject();
    }
}