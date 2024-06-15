using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Services.Schemas;
using AD.Api.Core.Schema;
using System.Collections;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;
using System.Text.Json;

namespace AD.Api.Core.Ldap
{
    [SupportedOSPlatform("WINDOWS")]
    [DependencyRegistration(Lifetime = ServiceLifetime.Transient)]
    public sealed class ResultEntry : IReadOnlyCollection<KeyValuePair<string, object>>
    {
        private readonly ISchemaService _schemas;
        private readonly SortedDictionary<string, object> _attributes;

        public int Count => _attributes.Count;

        public ResultEntry(ISchemaService schemaService)
        {
            _attributes = new(StringComparer.OrdinalIgnoreCase);
            _schemas = schemaService;
        }

        public void AddResult(string domain, SearchResultEntry entry)
        {
            ref readonly SchemaClassPropertyDictionary schema = ref _schemas[domain];
            foreach (DirectoryAttribute attribute in entry.Attributes.Values)
            {
                if (!schema.TryGetValue(attribute.Name, out SchemaProperty property) || property.IsEmpty)
                {
                    object value = attribute.Count switch
                    {
                        0 => string.Empty,
                        1 => attribute.GetValues(typeof(string))[0],
                        _ => attribute.GetValues(typeof(string)),
                    };

                    _ = _attributes.TryAdd(attribute.Name, value);
                    continue;
                }

                object rawValue = ConvertObject(attribute, property) ?? string.Empty;
                _ = _attributes.TryAdd(attribute.Name, rawValue);
            }
        }

        private static object? ConvertObject(DirectoryAttribute attribute, SchemaProperty property)
        {
            if (attribute.Count == 0)
            {
                return null;
            }

            Type type = property.RuntimeType;
            if (type.IsArray)
            {
                type = type.GetElementType()!;
            }

            if (type.Equals(typeof(byte[])) || type.Equals(typeof(Guid)))
            {
                return ConvertObject(attribute.GetValues(typeof(byte[]))[0] as object[] ?? [], property.RuntimeType);
            }
            else
            {
                return ConvertObject(attribute.GetValues(typeof(string)), property.RuntimeType);
            }
        }
        private static object? ConvertObject(object values, Type convertTo)
        {
            if (convertTo.IsArray && values is object[] objArr)
            {
                Type element = convertTo.GetElementType() ?? typeof(object);
                Array array = Array.CreateInstance(element, objArr.Length);
                for (int i = 0; i < objArr.Length; i++)
                {
                    array.SetValue(ConvertObject(objArr[i], element), i);
                }

                return array;
            }
            else if (values is object[] objArr2 && objArr2.Length == 1)
            {
                return ConvertObject(objArr2[0], convertTo);
            }
            else if (convertTo.Equals(typeof(string)))
            {
                return GetString(values);
            }
            else if (convertTo.Equals(typeof(byte[])))
            {
                return values;
            }
            else if (convertTo.Equals(typeof(Guid)))
            {
                return GetGuid(values);
            }
            else if (convertTo.Equals(typeof(bool?)))
            {
                return GetBoolean(values);
            }
            else if (convertTo.Equals(typeof(int?)))
            {
                return GetInt32(values);
            }
            else if (convertTo.Equals(typeof(long?)))
            {
                return GetInt64(values);
            }
            else if (convertTo.Equals(typeof(DateTime?)))
            {
                return GetDateTime(values);
            }

            return null;
        }
        
        private static bool? GetBoolean(object value)
        {
            return value is not string s || !bool.TryParse(s, out bool boolean) ? null : (bool?)boolean;
        }
        private static byte[] GetByteArray(object value)
        {
            return value is byte[] byteArray ? byteArray : [];
        }
        private static DateTime? GetDateTime(object value)
        {
            return value is not string s || !DateTime.TryParse(s, out DateTime date) ? null : (DateTime?)date;
        }
        private static Guid? GetGuid(object value)
        {
            return value is byte[] byteArray && byteArray.Length == 16 ? new Guid(byteArray) : null;
        }
        private static int? GetInt32(object value)
        {
            return value is not string s || !int.TryParse(s, out int number) ? null : (int?)number;
        }
        private static long? GetInt64(object value)
        {
            return value is not string s || !long.TryParse(s, out long number) ? null : (long?)number;
        }
        private static string GetString(object value)
        {
            return value is string s ? s : string.Empty;
        }

        public void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            if (_attributes.Count <= 0)
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
                return;
            }

            JsonSerializer.Serialize(writer, _attributes, options);
        }
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

