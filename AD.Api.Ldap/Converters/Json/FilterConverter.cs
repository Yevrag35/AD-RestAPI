using AD.Api.Ldap.Exceptions;
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
                statement = this.ReadToken(kvp, reader, serializer, out bool doContinue, true);
                if (statement?.Type == FilterType.Equal)
                {
                    statement = new And
                    {
                        statement
                    };
                }

                break;
            }

            return statement;
        }

        private IFilterStatement? ReadToken(KeyValuePair<string, JToken?> kvp, JsonReader reader, JsonSerializer serializer, out bool doContinue, bool start = false)
        {
            doContinue = false;
            IFilterStatement? statement = null;

            FilterType? keyword = null;
            if (Enum.TryParse(kvp.Key, true, out FilterType result))
                keyword = result;

            switch (keyword)
            {
                case FilterType.And:
                    statement = this.ReadAnd(kvp.Value, reader, serializer);
                    break;

                case FilterType.Or:
                    statement = this.ReadOr(kvp.Value, reader, serializer);
                    break;

                case FilterType.Not:
                    statement = this.ReadNot(kvp.Value, reader, serializer);
                    break;

                case FilterType.Nor:
                    statement = this.ReadNor(kvp.Value, reader, serializer);
                    break;

                case FilterType.Band:
                    statement = this.ReadBand(kvp.Value, reader, serializer);
                    break;

                case FilterType.Bor:
                    statement = this.ReadBor(kvp.Value, reader, serializer);
                    break;

                default:
                {
                    if (start)
                        doContinue = true;

                    if (kvp.Value is not null && kvp.Value.Type == JTokenType.Null)
                        statement = new Equal(kvp.Key, null);

                    else if (kvp.Value is IConvertible icon)
                        statement = new Equal(kvp.Key, icon);

                    else
                        throw new LdapFilterParsingException($"When not using a {nameof(FilterType)}, the only valid JSON is a Name/Value property.");

                    break;
                }
            }

            return statement;
        }

        private IFilterStatement? ReadAnd(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapFilterParsingException($"'{nameof(FilterType.And)}' must be followed by a JSON object.", FilterType.And);

            JObject job = (JObject)token;
            var and = new And();

            foreach (var kvp in job)
            {
                var filter = this.ReadToken(kvp, reader, serializer, out bool throwAway);
                and.Add(filter);
            }

            return and;
        }

        private IFilterStatement? ReadBand(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapFilterParsingException($"'{nameof(FilterType.Band)}' must be followed by a JSON object.", FilterType.Band);

            if (token is JObject job && job.Count == 1 && job.First is JProperty jprop)
                return new BitwiseAnd(jprop.Name, jprop.Value.ToObject<long>());

            else
                return null;
        }

        private IFilterStatement? ReadBor(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapFilterParsingException($"'{nameof(FilterType.Bor)}' must be followed by a JSON object.", FilterType.Bor);

            if (token is JObject job && job.Count == 1 && job.First is JProperty jprop)
                return new BitwiseOr(jprop.Name, jprop.Value.ToObject<long>());

            else
                return null;
        }

        private IFilterStatement? ReadNot(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Object)
                return null;

            if (token is JObject job)
            {
                if (job.Count > 1)
                {
                    var jar = new JArray();
                    foreach (var kvp in job)
                    {
                        if (kvp.Value is not null)
                            jar.Add(kvp.Value);
                    }

                    return this.ReadToken(new KeyValuePair<string, JToken?>("nor", jar), reader, serializer, out bool throwAway);
                }

                if (job.First is JProperty jProp)
                {
                    return new Not(jProp.Name, jProp.Value as IConvertible);
                }
            }

            return null;
        }

        private IFilterStatement? ReadNor(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Array)
                throw new LdapFilterParsingException($"'{nameof(FilterType.Nor)}' must be followed by a JSON array of objects.", FilterType.Nor);

            var jar = (JArray)token;

            var nor = new Nor();

            foreach (JToken? tok in jar)
            {
                if (tok is null || tok.Type != JTokenType.Object)
                    continue;
                
                JObject job = (JObject)tok;

                foreach (var kvp in job)
                {
                    var filter = this.ReadToken(kvp, reader, serializer, out bool throwAway);
                    nor.Add(filter);
                }
            }

            return nor;
        }

        private IFilterStatement? ReadOr(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Array)
                throw new LdapFilterParsingException($"'{nameof(FilterType.Or)}' must be followed by a JSON array.", FilterType.Or);

            JArray jar = (JArray)token;

            var or = new Or();
            foreach (var tok in jar)
            {
                if (tok.Type != JTokenType.Object)
                    continue;

                JObject job = (JObject)tok;

                foreach (var kvp in job)
                {
                    var filter = this.ReadToken(kvp, reader, serializer, out bool throwAway);
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
