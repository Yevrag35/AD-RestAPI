using AD.Api.Ldap.Exceptions;
using AD.Api.Ldap.Extensions;
using AD.Api.Ldap.Operations;
using AD.Api.Ldap.Operations.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AD.Api.Ldap.Converters
{
    public partial class OperationJsonConverter
    {
        private static IEnumerable<ILdapOperation>? ReadToken(string key, JToken? value)
        {
            OperationType? keywordType = null;
            if (Enum.TryParse(key, true, out OperationType result))
                keywordType = result;

            switch (keywordType)
            {
                case OperationType.Add:
                    return ReadValueOperation(value, key => new Add(key));

                case OperationType.Set:
                    return ReadValueOperation(value, key => new Set(key));
                    
                case OperationType.Remove:
                    return ReadValueOperation(value, key => new Remove(key));
                    
                case OperationType.Replace:
                    return ReadReplaceOperation(value, (key, comparer) => new Replace(key, comparer));

                default:
                    throw new LdapOperationgParsingException($"An edit operation requires at least one of the following keywords: \"{Enum.GetNames<OperationType>().Join()}\"");
            }
        }

        private static  object[]? ReadMultipleValues(JToken token)
        {
            return token.ToObject<object[]>();
        }
        private static object? ReadSingleValue(JToken token)
        {
            return token.ToObject<object>();
        }

        private static void ReadIntoValues(ILdapOperationWithValues operation, JToken? token)
        {
            if (token is null || token.Type == JTokenType.Null)
                return;

            switch (token.Type)
            {
                case JTokenType.Array:
                    object[]? arr = ReadMultipleValues(token);

                    if (!(arr is null))
                        operation.Values.AddRange(arr);

                    break;

                case JTokenType.None:
                case JTokenType.Null:
                case JTokenType.Comment:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Object:
                case JTokenType.Undefined:
                    throw new LdapOperationgParsingException($"A property of '{operation.OperationType}' must be a single or array of values.", operation.OperationType);

                default:
                    object? value = ReadSingleValue(token);
                    if (!(value is null))
                        operation.Values.Add(value);
                    break;
            }
        }

        private static IEnumerable<Replace>? ReadReplaceOperation(JToken? token, Func<string, IEqualityComparer<object>, Replace> replaceImplementation)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapOperationgParsingException($"A value type operation must be followed by a JSON object.");

            JObject job = (JObject)token;

            foreach (var kvp in job)
            {
                Replace replace = replaceImplementation(kvp.Key, Cache.Value[typeof(string)]);
                if (kvp.Value is null || kvp.Value.Type != JTokenType.Object)
                    continue;

                foreach (var replacePair in (JObject)kvp.Value)
                {
                    if (replacePair.Value is null)
                        continue;

                    switch (replacePair.Value.Type)
                    {
                        case JTokenType.None:
                        case JTokenType.Null:
                        case JTokenType.Comment:
                        case JTokenType.Constructor:
                        case JTokenType.Property:
                        case JTokenType.Object:
                        case JTokenType.Array:
                        case JTokenType.Undefined:
                            throw new LdapOperationgParsingException($"A property's value, in a '{nameof(Replace)}' operation, must be a single non-null value.", OperationType.Replace);

                        default:
                        {
                            replace.Values.Add((replacePair.Key, replacePair.Value.ToObject<object>()));
                            break;
                        }
                    }
                }

                yield return replace;
            }
        }

        private static IEnumerable<ILdapOperation>? ReadValueOperation(JToken? token, Func<string, ILdapOperationWithValues> factoryFunction)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapOperationgParsingException($"A value type operation must be followed by a JSON object.");

            JObject job = (JObject)token;

            foreach (KeyValuePair<string, JToken?> kvp in job)
            {
                ILdapOperationWithValues operation = factoryFunction(kvp.Key);
                ReadIntoValues(operation, kvp.Value);

                if (operation.Values.Count > 0)
                    yield return operation;
            }
        }
    }
}
