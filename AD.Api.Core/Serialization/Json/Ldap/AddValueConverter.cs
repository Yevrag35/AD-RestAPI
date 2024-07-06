using AD.Api.Core.Operations;
using AD.Api.Strings.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization.Json.Ldap;

public sealed class AddValueConverter : JsonConverter<AddDictionary>
{
    private const string FALSE = "0";
    private const string TRUE = "1";

    public override AddDictionary? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject || !reader.Read())
        {
            return null;
        }

        AddDictionary adds = [];
        while (reader.TokenType == JsonTokenType.PropertyName)
        {
            this.ReadProperty(ref reader, adds, options);
        }

        return adds;
    }

    private void ReadProperty(ref Utf8JsonReader reader, AddDictionary adds, JsonSerializerOptions options)
    {
        string key = reader.GetString() ?? throw new JsonException("Property keys cannot be empty.");
        if (!reader.Read())
        {
            throw new JsonException("Unexpected end of JSON object.");
        }

        object value = this.ReadValue(ref reader, key, options);
        adds.TryAdd(key, value);
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            reader.Read();
        }
    }

    private object ReadValue(ref Utf8JsonReader reader, string key, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.TryGetGuid(out Guid guid) ? guid.ToByteArray() : reader.GetString().OrEmpty();

            case JsonTokenType.True:
                return TRUE;

            case JsonTokenType.False:
                return FALSE;

            case JsonTokenType.Number:
                object val = reader.TryGetInt64(out long longValue) ? longValue : reader.GetDecimal();
                return val.ToString().OrEmpty();

            case JsonTokenType.StartArray:
                return JsonSerializer.Deserialize<string[]>(ref reader, options) ?? [];

            case JsonTokenType.None:
            case JsonTokenType.Null:
                throw new JsonException("Unexpected null value is Add operation.");

            default:
                throw new JsonException("Unexpected token in Add operation.");
        }
    }

    public override void Write(Utf8JsonWriter writer, AddDictionary value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize<Dictionary<string, object>>(writer, value, options);
    }
}