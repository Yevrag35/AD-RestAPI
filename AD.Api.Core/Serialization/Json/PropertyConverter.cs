using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Schema;
using AD.Api.Core.Settings;
using AD.Api.Statics;
using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;
using FrozenDict = System.Collections.Frozen.FrozenDictionary<string, AD.Api.Core.Serialization.SerializerAction>;

namespace AD.Api.Core.Serialization.Json
{
    public sealed class PropertyConverter
    {
        private readonly FrozenDict _dictionary;
        private IServiceScopeFactory _scopeFactory = null!;

        private PropertyConverter(ConversionDictionary dictionary)
        {
            _dictionary = dictionary.ToFrozen();
        }

        public void AddScopeFactory(IServiceScopeFactory scopeFactory)
        {
            Debug.Assert(_scopeFactory is null);
            _scopeFactory = scopeFactory;
        }

        public void WriteTo(Utf8JsonWriter writer, ResultEntryCollection collection, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            if (collection.Count == 0)
            {
                writer.WriteEndArray();
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            SerializationContext context = GetContext(scope.ServiceProvider, options);

            foreach (var entry in collection)
            {
                writer.WriteStartObject();
                if (entry.Count == 0)
                {
                    writer.WriteEndObject();
                    continue;
                }

                foreach (var kvp in entry)
                {
                    this.WriteTo(writer, in kvp, ref context);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
        public void WriteTo(Utf8JsonWriter writer, ResultEntry result, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (result.Count <= 0)
            {
                writer.WriteEndObject();
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            SerializationContext context = GetContext(scope.ServiceProvider, options);

            foreach (var kvp in result)
            {
                this.WriteTo(writer, in kvp, ref context);
            }

            writer.WriteEndObject();
        }
        private void WriteTo(Utf8JsonWriter writer, in KeyValuePair<string, object> pair, ref SerializationContext context)
        {
            writer.WritePropertyName(pair.Key);
            if (_dictionary.TryGetValue(pair.Key, out var action))
            {
                context.AttributeName = pair.Key;
                context.Value = pair.Value;
                action(writer, ref context);
            }
            //else if (_guidAttributes.Contains(pair.Key) && TryConvertToGuid(pair.Value, out Guid guidValue))
            //{
            //    Span<char> chars = stackalloc char[LengthConstants.GUID_FORM_D];
            //    _ = guidValue.TryFormat(chars, out int charsWritten);
            //    writer.WriteStringValue(chars.Slice(0, charsWritten));
            //}
            else
            {
                JsonSerializer.Serialize(writer, pair.Value,
                    pair.Value?.GetType() ?? SchemaProperty.ObjectType, context.Options);
            }
        }

        private static SerializationContext GetContext(IServiceProvider provider, JsonSerializerOptions options)
        {
            return new(options, provider);
        }

        public static PropertyConverter AddToServices(IServiceCollection services, IConfiguration configuration, Action<IConversionDictionary> addConversions)
        {
            ConversionDictionary dict = new();
            addConversions(dict);

            PropertyConverter converter = new(dict);
            services.AddSingleton(converter);

            return converter;
        }
    }
}

