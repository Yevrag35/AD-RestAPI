using MG.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Components;
using AD.Api.Exceptions;
using AD.Api.Extensions;
using System.Reflection;

namespace AD.Api.Models.Converters
{
    [SupportedOSPlatform("windows")]
    public class PropertyMethodConverter<T, TCol> : JsonConverter<PropertyMethod<T>>
        where TCol : IValueCollection<T>, new()
    {
        public override PropertyMethod<T> ReadJson(JsonReader reader, Type objectType, PropertyMethod<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var pm = new PropertyMethod<T>();
            if (reader.TokenType == JsonToken.StartObject && reader.Read())
            {
                while (reader.TokenType != JsonToken.EndObject)
                {
                    if (JToken.ReadFrom(reader) is not JProperty jprop)
                        break;

                    switch (jprop.Name)
                    {
                        case "operation":
                            pm.Operation = Enum.TryParse(jprop.Value.ToObject<string>(), true, out Operation result)
                                ? result
                                : Operation.Set;
                            break;

                        case "oldValues":
                            AddListToProperty(jprop, pm, (pmv, list) => pm.OldValues = list);
                            break;

                        case "newValues":
                            AddListToProperty(jprop, pm, (pmv, list) => pm.NewValues = list);
                            break;
                    }
                }
            }

            pm.CheckOperation();
            return pm;
        }
        public override void WriteJson(JsonWriter writer, PropertyMethod<T> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            value.NewValues?.ForEach((s) =>
            {
                writer.WriteValue(s);
            });

            writer.WriteEndArray();
        }
        private static void AddListToProperty(JProperty jProperty, PropertyMethod<T> pm, Action<PropertyMethod<T>, IValueCollection<T>> action)
        {
            var list = new TCol();
            if (jProperty.Value is JArray jar)
            {
                foreach (T val in jar.Values<T>())
                {
                    list.Add(val);
                }
            }
            else
            {
                list.Add(jProperty.Value.ToObject<T>());
            }

            action(pm, list);
        }
    }
}
