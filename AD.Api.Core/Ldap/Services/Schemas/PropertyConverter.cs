using System.Collections.Frozen;
using System.Text.Json;
using FrozenDict = System.Collections.Frozen.FrozenDictionary<string, System.Action<System.Text.Json.Utf8JsonWriter, System.Text.Json.JsonSerializerOptions, object>>;
using PropDict = System.Collections.Generic.Dictionary<string, System.Action<System.Text.Json.Utf8JsonWriter, System.Text.Json.JsonSerializerOptions, object>>;

namespace AD.Api.Core.Ldap.Services.Schemas
{
    public sealed class PropertyConverter
    {
        private readonly FrozenDict _dictionary;

        private PropertyConverter(PropDict dictionary)
        {
            _dictionary = dictionary.ToFrozenDictionary(dictionary.Comparer);
        }

        public void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options, in KeyValuePair<string, object> pair)
        {
            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(pair.Key) ?? pair.Key);
            if (_dictionary.TryGetValue(pair.Key, out var action))
            {
                action(writer, options, pair.Value);
            }
            else
            {
                JsonSerializer.Serialize(writer, pair.Value, options);
            }
        }

        public static void AddToServices(IServiceCollection services, Action<PropDict> addConversions)
        {
            PropDict dict = new(10, StringComparer.OrdinalIgnoreCase);
            addConversions(dict);
            services.AddSingleton(new PropertyConverter(dict));
        }
    }
}

