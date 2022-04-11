using AD.Api.Ldap.Exceptions;
using AD.Api.Ldap.Extensions;
using AD.Api.Ldap.Operations;
using AD.Api.Ldap.Operations.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    return ReadValueOperation(keywordType.Value, value, key => new Add(key));

                case OperationType.Set:
                    return ReadValueOperation(keywordType.Value, value, key => new Set(key));
                    
                case OperationType.Remove:
                    return ReadValueOperation(keywordType.Value, value, key => new Remove(key));
                    
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
        private static T[]? ReadMultipleValues<T>(JToken token)
        {
            return token.ToObject<T[]>();
        }
        private static IEnumerable<ILdapOperation> ReadProxyAddress(OperationType type, JToken? token)
        {
            if (token is null)
                yield break;

            switch (token.Type)
            {
                case JTokenType.Array:
                    var paOp = new ProxyAddressesOperation(type);
                    string[]? arr = ReadMultipleValues<string>(token);

                    if (!(arr is null))
                        paOp.Values.AddRange(arr);

                    yield return paOp;

                    break;

                case JTokenType.Object:
                {
                    JObject job = (JObject)token;
                    var props = job.Children<JProperty>();
                    yield return new ProxyAddressesOperation(OperationType.Add, props.Select(x => x.Value.ToObject<string>() ?? string.Empty));
                    yield return new ProxyAddressesOperation(OperationType.Remove, props.Select(x => x.Name));
                    break;
                }

                case JTokenType.None:
                case JTokenType.Null:
                case JTokenType.Comment:
                case JTokenType.Constructor:
                case JTokenType.Property:
                
                case JTokenType.Undefined:
                    throw new LdapOperationgParsingException($"A property of '{type}' must be a single or array of values.", type);

                default:
                    var paOp2 = new ProxyAddressesOperation(type);
                    string? value = ReadSingleValue<string>(token);
                    if (!string.IsNullOrWhiteSpace(value))
                        paOp2.Values.Add(value);
                    yield return paOp2;
                    break;
            }
        }
        private static object? ReadSingleValue(JToken token)
        {
            return token.ToObject<object>();
        }
        private static T? ReadSingleValue<T>(JToken token)
        {
            return token.ToObject<T>();
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

        private static IEnumerable<ILdapOperation>? ReadReplaceOperation(JToken? token, Func<string, IEqualityComparer<object>, Replace> replaceImplementation)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapOperationgParsingException($"A value type operation must be followed by a JSON object.");

            JObject job = (JObject)token;

            foreach (var kvp in job)
            {
                if (!(kvp.Value is null) && kvp.Key.Equals(ProxyAddressesOperation.PROPERTY, StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var op in ReadProxyAddress(OperationType.Replace, kvp.Value))
                    {
                        yield return op;
                    }

                    continue;
                }

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

        private static IEnumerable<ILdapOperation>? ReadValueOperation(OperationType type, JToken? token, Func<string, ILdapOperationWithValues> factoryFunction)
        {
            if (token is null || token.Type != JTokenType.Object)
                throw new LdapOperationgParsingException($"A value type operation must be followed by a JSON object.");

            JObject job = (JObject)token;

            foreach (KeyValuePair<string, JToken?> kvp in job)
            {
                if (kvp.Key.Equals(ProxyAddressesOperation.PROPERTY, StringComparison.CurrentCultureIgnoreCase))
                {
                    IEnumerable<ILdapOperation>? paOperation = ReadProxyAddress(type, kvp.Value);
                    if (!(paOperation is null))
                    {
                        foreach (var op in paOperation)
                        {
                            yield return op;
                        }

                        continue;
                    }
                }

                ILdapOperationWithValues operation = factoryFunction(kvp.Key);
                ReadIntoValues(operation, kvp.Value);

                if (operation.Values.Count > 0)
                    yield return operation;
            }
        }
    }
}
