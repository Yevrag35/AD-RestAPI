using AD.Api.Ldap.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AD.Api.Ldap.Converters.Json
{
    public class FilterConverter : JsonConverter<IFilterStatement>
    {
        public override IFilterStatement? ReadJson(JsonReader reader, Type objectType, IFilterStatement? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return existingValue;

            JObject job = (JObject)JToken.ReadFrom(reader);

            IFilterStatement? statement = null;
            foreach (var kvp in job)
            {
                statement = this.ReadToken(kvp, reader, serializer);
                break;
            }

            return statement;
        }

        private IFilterStatement? ReadToken(KeyValuePair<string, JToken?> kvp, JsonReader reader, JsonSerializer serializer)
        {
            IFilterStatement? statement = null;

            switch (kvp.Key)
            {
                case "And":
                case "and":
                    statement = this.ReadAnd(kvp.Value, reader, serializer);
                    break;

                case "Or":
                case "or":
                    statement = this.ReadOr(kvp.Value, reader, serializer);
                    break;

                default:
                {
                    if (kvp.Value?.Type == JTokenType.Null)
                        statement = new Equal(kvp.Key, null);

                    else if (kvp.Value is IConvertible icon)
                        statement = new Equal(kvp.Key, icon);

                    break;
                }
            }

            return statement;
        }

        private IFilterStatement? ReadAnd(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Object)
                return null;

            JObject job = (JObject)token;
            var and = new And();

            foreach (var kvp in job)
            {
                var filter = this.ReadToken(kvp, reader, serializer);
                and.Add(filter);
            }

            return and;
        }

        private IFilterStatement? ReadOr(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Array)
                return null;

            JArray jar = (JArray)token;

            var or = new Or();
            foreach (var tok in jar)
            {
                if (tok.Type != JTokenType.Object)
                    continue;

                JObject job = (JObject)tok;

                foreach (var kvp in job)
                {
                    var filter = this.ReadToken(kvp, reader, serializer);
                    or.Add(filter);
                }
            }

            return or;
        }

        public override void WriteJson(JsonWriter writer, IFilterStatement? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
