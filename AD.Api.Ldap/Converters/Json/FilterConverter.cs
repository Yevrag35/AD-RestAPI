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
                statement = this.ReadToken(kvp, reader, serializer);
                break;
            }

            return statement;
        }

        private IFilterStatement? ReadToken(KeyValuePair<string, JToken?> kvp, JsonReader reader, JsonSerializer serializer)
        {
            IFilterStatement? statement = null;

            FilterKeyword? keyword = null;
            if (Enum.TryParse(kvp.Key, true, out FilterKeyword result))
                keyword = result;

            switch (keyword)
            {
                case FilterKeyword.And:
                    statement = this.ReadAnd(kvp.Value, reader, serializer);
                    break;

                case FilterKeyword.Or:
                    statement = this.ReadOr(kvp.Value, reader, serializer);
                    break;

                case FilterKeyword.Not:
                    statement = this.ReadNot(kvp.Value, reader, serializer);
                    break;

                case FilterKeyword.Band:
                    statement = this.ReadBand(kvp.Value, reader, serializer);
                    break;

                case FilterKeyword.Bor:
                    statement = this.ReadBor(kvp.Value, reader, serializer);
                    break;

                default:
                {
                    if (kvp.Value is not null && kvp.Value.Type == JTokenType.Null)
                        statement = new Equal(kvp.Key, null);

                    else if (kvp.Value is IConvertible icon)
                        statement = new Equal(kvp.Key, icon);

                    else
                        throw new LdapFilterParsingException($"When not using a {nameof(FilterKeyword)}, the only valid JSON is a Name/Value property.");

                    break;
                }
            }

            return statement;
        }

        private IFilterStatement? ReadAnd(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapFilterParsingException($"'{nameof(FilterKeyword.And)}' must be followed by a JSON object.", FilterKeyword.And);

            JObject job = (JObject)token;
            var and = new And();

            foreach (var kvp in job)
            {
                var filter = this.ReadToken(kvp, reader, serializer);
                and.Add(filter);
            }

            return and;
        }

        private IFilterStatement? ReadBand(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapFilterParsingException($"'{nameof(FilterKeyword.Band)}' must be followed by a JSON object.", FilterKeyword.Band);

            if (token is JObject job && job.Count == 1 && job.First is JProperty jprop)
                return new BitwiseAnd(jprop.Name, jprop.Value.ToObject<long>());

            else
                return null;
        }

        private IFilterStatement? ReadBor(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapFilterParsingException($"'{nameof(FilterKeyword.Bor)}' must be followed by a JSON object.", FilterKeyword.Bor);

            if (token is JObject job && job.Count == 1 && job.First is JProperty jprop)
                return new BitwiseOr(jprop.Name, jprop.Value.ToObject<long>());

            else
                return null;
        }

        private IFilterStatement? ReadNot(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Array)
                throw new LdapFilterParsingException($"'{nameof(FilterKeyword.Not)}' must be followed by a JSON array.", FilterKeyword.Not);

            JArray jar = (JArray)token;
            var not = new Not();

            foreach (var tok in jar)
            {
                if (tok.Type != JTokenType.Object)
                    continue;

                JObject job = (JObject)tok;

                foreach (var kvp in job)
                {
                    var filter = this.ReadToken(kvp, reader, serializer);
                    not.Add(filter);
                }
            }

            return not;
        }

        private IFilterStatement? ReadOr(JToken? token, JsonReader reader, JsonSerializer serializer)
        {
            if (token is null || token.Type != JTokenType.Array)
                throw new LdapFilterParsingException($"'{nameof(FilterKeyword.Or)}' must be followed by a JSON array.", FilterKeyword.Or);

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
