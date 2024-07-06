using AD.Api.Components;
using AD.Api.Core.Operations;
using System.Buffers;
using System.Text.Json;

namespace AD.Api.Core.Serialization.Json.Ldap
{
    public sealed class ReplaceOperationConverter : EditOperationConverter<ReplaceDictionary, DirectoryAttributeModification[]>
    {
        protected override JsonTokenType StartingTokenType => JsonTokenType.StartObject;
        
        protected override ReplaceDictionary CreateCollection()
        {
            return [];
        }

        protected override void Deserialize(ref Utf8JsonReader reader, ReplaceDictionary collection, JsonSerializerOptions options)
        {
            var array = ArrayPool<OneOf<string, byte[], string[]>>.Shared.Rent(2);
            var span = array.AsSpan(0, 2);
            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                string key = reader.GetString() ?? throw new JsonException("Property keys cannot be null or empty");
                if (!reader.Read())
                {
                    throw new JsonException($"Unexpected end of JSON object after property: {key}");
                }
                else if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException($"Expected start of object in property: {key}");
                }

                var replaceValues = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options) ?? [];
                if (replaceValues.Count == 0)
                {
                    throw new JsonException("Expected at least one old/new value in replace operation.");
                }
                else if (replaceValues.Count == 1)
                {
                    span[0] = replaceValues.Keys.First();
                    span[1] = replaceValues.Values.FirstOrDefault()?.ToString() ?? throw new JsonException("Expected non-null value in replace operation.");
                }
                else
                {
                    span[0] = replaceValues.Keys.ToArray();
                    span[1] = replaceValues.Values.Select(x => x?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                }

                collection.Add(key, span[0], span[1]);
                reader.Read();
            }

            ArrayPool<OneOf<string, byte[], string[]>>.Shared.Return(array);
        }
        public override void Write(Utf8JsonWriter writer, ReplaceDictionary value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

