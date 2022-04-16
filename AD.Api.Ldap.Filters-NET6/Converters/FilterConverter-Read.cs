using AD.Api.Ldap.Exceptions;
using AD.Api.Ldap.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AD.Api.Ldap.Converters
{
    public partial class FilterConverter
    {
        public override IFilterStatement? ReadJson(JsonReader reader, Type objectType, IFilterStatement? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return existingValue;

            JObject job = (JObject)JToken.ReadFrom(reader);

            IFilterStatement? statement = null;
            foreach (var kvp in job)
            {
                statement = this.ReadToken(kvp, out bool doContinue, true);
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

        private IFilterStatement? ReadToken(KeyValuePair<string, JToken?> kvp, out bool doContinue, bool start = false)
        {
            doContinue = false;
            IFilterStatement? statement = null;

            FilterType? keyword = null;
            if (Enum.TryParse(kvp.Key, true, out FilterType result))
                keyword = result;

            switch (keyword)
            {
                case FilterType.And:
                    statement = this.ReadAnd(kvp.Value);
                    break;

                case FilterType.Or:
                    statement = this.ReadArray<Or>(kvp.Value, FilterType.Or);
                    break;

                case FilterType.Not:
                    statement = this.ReadNot(kvp.Value);
                    break;

                case FilterType.Nor:
                    statement = this.ReadArray<Nor>(kvp.Value, FilterType.Nor);
                    break;

                case FilterType.Band:
                    statement = this.ReadEquals(kvp.Value, FilterType.Band, prop =>
                    {
                        return new BitwiseAnd(prop.Name, prop.Value.ToObject<long>());
                    });
                    break;

                case FilterType.Bor:
                    statement = this.ReadEquals(kvp.Value, FilterType.Bor, prop =>
                    {
                        return new BitwiseOr(prop.Name, prop.Value.ToObject<long>());
                    });
                    break;

                case FilterType.Recurse:
                    statement = this.ReadEquals(kvp.Value, FilterType.Recurse, prop =>
                    {
                        return new Recurse(prop.Name, prop.Value.ToObject<string>() ?? string.Empty);
                    });
                    break;

                default:
                {
                    if (start)
                        doContinue = true;

                    if (!(kvp.Value is null) && kvp.Value.Type == JTokenType.Null)
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

        private IFilterStatement? ReadAnd(JToken? token)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapFilterParsingException($"'{nameof(FilterType.And)}' must be followed by a JSON object.", FilterType.And);

            JObject job = (JObject)token;
            var and = new And();

            foreach (var kvp in job)
            {
                var filter = this.ReadToken(kvp, out bool throwAway);
                and.Add(filter);
            }

            return and;
        }

        private T? ReadEquals<T>(JToken? token, FilterType type, Func<JProperty, T> newEqualFunction)
            where T : EqualityStatement
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapFilterParsingException($"'{typeof(T).Name}' must be followed by a JSON object.", type);

            return token is JObject job && job.Count == 1 && job.First is JProperty jProp
                ? newEqualFunction(jProp)
                : null;
        }

        private T ReadArray<T>(JToken? token, FilterType type) where T : FilterContainer, new()
        {
            if (token is null || token.Type != JTokenType.Array)
                throw new LdapFilterParsingException($"'{typeof(T).Name}' must be followed by a JSON array of objects.", type);

            var jar = (JArray)token;

            T container = new T();

            foreach (JToken? tok in jar)
            {
                if (tok is null || tok.Type != JTokenType.Object)
                    continue;

                foreach (var kvp in (JObject)tok)
                {
                    var filter = this.ReadToken(kvp, out bool throwAway);
                    container.Add(filter);
                }
            }

            return container;
        }

        private IFilterStatement? ReadNot(JToken? token)
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
                        if (!(kvp.Value is null))
                            jar.Add(kvp.Value);
                    }

                    return this.ReadToken(new KeyValuePair<string, JToken?>("nor", jar), out bool throwAway);
                }

                if (job.First is JProperty jProp)
                {
                    return new Not(jProp.Name, jProp.Value as IConvertible);
                }
            }

            return null;
        }
    }
}
