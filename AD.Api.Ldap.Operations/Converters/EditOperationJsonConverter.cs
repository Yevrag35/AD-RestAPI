using AD.Api.Ldap.Exceptions;
using AD.Api.Ldap.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace AD.Api.Ldap.Converters
{
    public partial class EditOperationJsonConverter : JsonConverter<List<ILdapOperation>>
    {
        private static readonly Lazy<ComparerCache> Cache = new();

        private NamingStrategy NamingStrategy { get; }

        public EditOperationJsonConverter(NamingStrategy? namingStrategy)
        {
            if (namingStrategy is null)
                namingStrategy = new CamelCaseNamingStrategy();

            this.NamingStrategy = namingStrategy;
        }

        public override List<ILdapOperation>? ReadJson(JsonReader reader, Type objectType, List<ILdapOperation>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return existingValue;

            if (existingValue is null)
                existingValue = new List<ILdapOperation>(1);

            JObject job = (JObject)JToken.ReadFrom(reader);

            foreach (var kvp in job)
            {
                IEnumerable<ILdapOperation>? operations = ReadToken(kvp.Key, kvp.Value);

                if (!(operations is null))
                    existingValue.AddRange(operations);
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, List<ILdapOperation>? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (!(value is null))
            {
                IEnumerable<IGrouping<OperationType, ILdapOperation>> grouping = value.GroupBy(x => x.OperationType);

                foreach (var group in grouping.OrderBy(x => x.Key))
                {
                    writer.WritePropertyName(this.NamingStrategy.GetPropertyName(group.Key.ToString(), false));

                    writer.WriteStartObject();

                    foreach (var operation in group)
                    {
                        operation.WriteTo(writer, this.NamingStrategy, serializer);
                    }

                    writer.WriteEndObject();
                }
            }

            writer.WriteEndObject();
        }
    }
}
