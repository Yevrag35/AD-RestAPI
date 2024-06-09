using AD.Api.Extensions;
using AD.Api.Ldap.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AD.Api.Ldap.Filters.Converters
{
    public sealed partial class FilterJsonConverter : JsonConverter<IFilterStatement>
    {
        public override IFilterStatement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                ThrowJsonEx();
            }

            JsonObject obj = JsonNode.Parse(ref reader, new JsonNodeOptions
            {
                PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive
            })?.AsObject() ?? ThrowJsonEx();

            IFilterStatement? statement = null;

            foreach (KeyValuePair<string, JsonNode?> item in obj)
            {
                statement = ReadToken(item, out bool doContinue, start: true);
                if (statement?.Type == FilterType.Equal)
                {
                    statement = new And
                    {
                        statement,
                    };
                }

                break;
            }

            return statement;
        }

        private static IFilterStatement? ReadToken(KeyValuePair<string, JsonNode?> kvp, out bool doContinue, bool start = false)
        {
            doContinue = false;
            IFilterStatement? statement = null;

            FilterType? keyword = null;
            if (Enum.TryParse(kvp.Key, ignoreCase: true, out FilterType result))
            {
                keyword = result;
            }

            switch (keyword)
            {
                case FilterType.And:
                    statement = ReadAnd(kvp.Value);
                    break;

                case FilterType.Or:
                    statement = ReadArray<Or>(kvp.Value, FilterType.Or);
                    break;

                case FilterType.Not:
                    statement = ReadNot(kvp.Value);
                    break;

                case FilterType.Nor:
                    statement = ReadArray<Nor>(kvp.Value, FilterType.Nor);
                    break;

                case FilterType.Band:
                    statement = ReadEquals(kvp.Value, FilterType.Band, kvp =>
                    {
                        return new BitwiseAnd(kvp.Key, kvp.Value.GetValue<long>());
                    });
                    break;

                case FilterType.Bor:
                    statement = ReadEquals(kvp.Value, FilterType.Bor, kvp =>
                    {
                        return new BitwiseOr(kvp.Key, kvp.Value.GetValue<long>());
                    });
                    break;

                case FilterType.Recurse:
                    statement = ReadEquals(kvp.Value, FilterType.Recurse, kvp =>
                    {
                        return new Recurse(kvp.Key, kvp.Value.GetValue<string>() ?? string.Empty);
                    });
                    break;

                default:
                    if (start)
                    {
                        doContinue = true;
                    }

                    if (kvp.Value.TryAsValue(out var value) && value.TryGetValue(out JsonElement ele))
                    {
                        switch (ele.ValueKind)
                        {
                            case JsonValueKind.String:
                                statement = new Equal(kvp.Key, ele.GetString());
                                break;

                            case JsonValueKind.Number:
                                statement = new Equal(kvp.Key, formattable: ele.GetInt64());
                                break;

                            case JsonValueKind.True:
                                statement = new Equal(kvp.Key, bool.TrueString);
                                break;

                            case JsonValueKind.False:
                                statement = new Equal(kvp.Key, bool.FalseString);
                                break;

                            default:
                                statement = new Equal(kvp.Key, formattable: null);
                                break;
                        }
                    }
                    else
                    {
                        statement = new Equal(kvp.Key, formattable: null);
                    } 

                    break;
            }

            return statement;
        }

        private static IFilterStatement? ReadAnd(JsonNode? node)
        {
            if (!node.TryAsObject(out var obj))
            {
                throw new LdapFilterParsingException($"'{nameof(FilterType.And)}' must be followed by a JSON object.", FilterType.And);
            }

            And and = new();
            foreach (var kvp in obj)
            {
                var filter = ReadToken(kvp, out bool _);
                and.Add(filter);
            }

            return and;
        }
        private static IFilterStatement? ReadArray<T>(JsonNode? node, FilterType type) where T : FilterContainer, new()
        {
            if (!node.TryAsArray(out var array))
            {
                throw new LdapFilterParsingException($"'{typeof(T).Name}' must be followed by a JSON array of objects.", type);
            }

            T container = new();
            foreach (JsonNode? subNode in array)
            {
                if (!subNode.TryAsObject(out var subObj))
                {
                    continue;
                }

                foreach (var kvp in subObj)
                {
                    var filter = ReadToken(kvp, out bool _);
                    container.Add(filter);
                }
            }

            return container;
        }
        private static IFilterStatement? ReadEquals<T>(JsonNode? node, FilterType type, Func<KeyValuePair<string, JsonValue>, T> newEqualFunction) where T : EqualityStatement
        {
            if (!node.TryAsObject(out var obj))
            {
                throw new LdapFilterParsingException($"'{typeof(T).Name}' must be followed by a JSON object.", type);
            }

            if (obj.Count == 1)
            {
                var kvp = obj.First();
                if (!kvp.Value.TryAsValue(out var firstVal))
                {
                    throw new LdapFilterParsingException($"'{typeof(T).Name}' must be followed by a JSON object.", type);
                }

                var newKvp = new KeyValuePair<string, JsonValue>(kvp.Key, firstVal);
                return newEqualFunction(newKvp);
            }
            else
            {
                return null;
            }
        }
        private static IFilterStatement? ReadNot(JsonNode? node)
        {
            if (!node.TryAsObject(out var obj))
            {
                return null;
            }

            if (obj.Count > 1)
            {
                JsonArray array = new();
                foreach (var kvp in obj)
                {
                    if (kvp.Value is not null)
                    {
                        array.Add(kvp.Value);
                    }
                }

                return ReadToken(new KeyValuePair<string, JsonNode?>("nor", array), out bool _);
            }

            var first = obj.FirstOrDefault();
            if (first.Value.TryAsValue(out var firstVal))
            {
                return new Not(first.Key, firstVal.GetValue<IConvertible>());
            }

            return null;
        }


        [DoesNotReturn]
        private static JsonObject ThrowJsonEx(ReadOnlySpan<char> filter = default)
        {
            string msg = "Invalid filter string provided.";
            if (!filter.IsWhiteSpace())
            {
                msg = msg + " " + filter.ToString();
            }

            throw new JsonException(msg);
        }

        public override void Write(Utf8JsonWriter writer, IFilterStatement value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
