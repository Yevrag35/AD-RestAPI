using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap;
using AD.Api.Core.Schema;
using AD.Api.Reflection;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace AD.Api.Core.Serialization
{
    public interface IAttributeConverter
    {
        void ConvertEntry<T>(string domainKey, SearchResultEntry result, [DisallowNull] T dictionary) where T : class, IDictionary<string, object>;
    }

    [DependencyRegistration(typeof(IAttributeConverter), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class AttributeConverter : IAttributeConverter
    {
        private const string DT_FORMAT = "yyyyMMddHHmmss.0'Z'";
        private static readonly Type _byteType;
        private static readonly Type _jaggedByteType;
        private static readonly GenericMethodDictionary<Type> _methodCache;
        private static readonly MethodInfo _resizeMethod;

        static AttributeConverter()
        {
            _byteType = typeof(byte);
            _jaggedByteType = typeof(byte[][]);
            _methodCache = new(Environment.ProcessorCount, 5);

            _resizeMethod = typeof(AttributeConverter)
                .GetMethod(nameof(Resize), BindingFlags.NonPublic | BindingFlags.Static)
                    ?? throw new MissingMethodException(nameof(AttributeConverter), nameof(Resize));
        }

        private readonly ISchemaService _schemaSvc;

        public AttributeConverter(ISchemaService schemaSvc)
        {
            _schemaSvc = schemaSvc;
        }

        public void ConvertEntry<T>(string domainKey, SearchResultEntry result, [DisallowNull] T dictionary) where T : class, IDictionary<string, object>
        {
            ref readonly SchemaClassPropertyDictionary schema = ref _schemaSvc[domainKey];
            foreach (DirectoryAttribute attribute in result.Attributes.Values)
            {
                ConvertAttribute(attribute, schema, dictionary);
            }
        }

        private static void ConvertAttribute<T>(DirectoryAttribute attribute, SchemaClassPropertyDictionary schemaDict, [DisallowNull] T dictionary) where T : class, IDictionary<string, object>
        {
            if (!schemaDict.TryGetValue(attribute.Name, out SchemaProperty property) || property.IsEmpty)
            {
                ConvertUnregisteredAttribute(attribute, dictionary);
                return;
            }

            object? rawValue = ConvertObject(attribute, in property);
            _ = dictionary.TryAdd(attribute.Name, rawValue ?? string.Empty);
        }

        private static void ConvertUnregisteredAttribute<T>(DirectoryAttribute attribute, [DisallowNull] T dictionary) where T : class, IDictionary<string, object>
        {
            object value = attribute.Count switch
            {
                <= 0 => string.Empty,
                1 => attribute.GetValues(SchemaProperty.StringType)[0],
                _ => attribute.GetValues(SchemaProperty.StringType),
            };

            _ = dictionary.TryAdd(attribute.Name, value);
        }

        #region OBJECT CONVERSION

        private static object? ConvertObject(DirectoryAttribute attribute, in SchemaProperty property)
        {
            if (attribute.Count == 0)
            {
                return null;
            }

            Type type = GetRuntimeType(in property);
            if (property.LdapType == LdapValueType.ByteArray || property.LdapType == LdapValueType.ByteTwoRankArray || 
                property.LdapType == LdapValueType.Guid || property.LdapType == LdapValueType.GuidArray)
            {
                object[] values = attribute.GetValues(SchemaProperty.ByteArrayType);
                return !property.IsMultiValued
                    ? ConvertObject(values[0], property.RuntimeType, property.LdapType)
                    : ConvertObject(values, property.RuntimeType, property.LdapType);
            }
            else
            {
                object[] values = attribute.GetValues(SchemaProperty.StringType);
                return !property.IsMultiValued && values.Length == 1
                    ? ConvertObject(values[0], property.RuntimeType, property.LdapType)
                    : ConvertObject(values, property.RuntimeType, property.LdapType);
            }
        }

        private static Array ConvertArrayObjects(Array array, Type convertTo, LdapValueType ldapType)
        {
            ldapType = ldapType switch
            {
                LdapValueType.BooleanArray => LdapValueType.Boolean,
                LdapValueType.StringArray => LdapValueType.String,
                LdapValueType.IntegerArray => LdapValueType.Integer,
                LdapValueType.LongArray => LdapValueType.Long,
                LdapValueType.DateTimeArray => LdapValueType.DateTime,
                LdapValueType.GuidArray => LdapValueType.Guid,
                LdapValueType.ObjectArray => LdapValueType.Object,
                _ => LdapValueType.Object,
            };

            return ConvertArray(array, convertTo.GetElementType()!, in ldapType);
        }
        private static object? ConvertObject(object? values, Type convertTo, LdapValueType ldapType)
        {
            switch (ldapType)
            {
                case LdapValueType.String:
                    return GetString(values);

                case LdapValueType.ByteArray:
                    return GetByteArray(values);

                case LdapValueType.Guid:
                    return GetGuid(values);

                case LdapValueType.Boolean:
                    return GetBoolean(values);

                case LdapValueType.Integer:
                    return GetInt32(values);

                case LdapValueType.Long:
                    return GetInt64(values);

                case LdapValueType.DateTime:
                    return GetDateTime(values);

                case LdapValueType.ByteTwoRankArray:
                    return (byte[][])values!;

                case LdapValueType.StringArray:
                case LdapValueType.BooleanArray:
                case LdapValueType.IntegerArray:
                case LdapValueType.LongArray:
                case LdapValueType.GuidArray:
                case LdapValueType.DateTimeArray:
                case LdapValueType.ObjectArray:
                    return ConvertArrayObjects((Array)values!, convertTo, ldapType);

                default:
                    return values;
            }
        }
        private static MethodInfo GetResizeMethod(Type elementType)
        {
            if (!_methodCache.TryGetValue(elementType, out MethodInfo? method))
            {
                method = _resizeMethod.MakeGenericMethod(elementType);
                _ = _methodCache.TryAdd(elementType, method);
            }

            return method;
        }
        private static Type GetRuntimeType(in SchemaProperty property)
        {
            Type type = property.RuntimeType;
            if (type.IsArray)
            {
                type = type.GetElementType()!;
            }

            return type;
        }
        private static void Resize<T>(ref Array array, int newSize)
        {
            T[] tArr = Unsafe.As<T[]>(array);
            Array.Resize(ref tArr, newSize);
            array = tArr;
        }
        private static void ResizeNonGeneric(Type elementType, int newSize, ref Array nonGenArray)
        {
            MethodInfo method = GetResizeMethod(elementType);
            object[] parameters = [nonGenArray, newSize];
            try
            {
                method.Invoke(null, parameters);
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
                throw;
            }

            nonGenArray = (Array)parameters[0];
        }
        private static bool TryGetElementType([NotNullWhen(true)] object? values, Type type, out Array array, [NotNullWhen(true)] out Type? elementType)
        {
            if (!type.IsArray || values is not Array objArray)
            {
                elementType = null;
                array = Array.Empty<object>();
                return false;
            }

            elementType = type.GetElementType()!;
            array = objArray;
            return true;
        }

        #endregion

        #region VALUE CONVERSIONS
        private static Array ConvertArray(Array objArray, Type elementType, in LdapValueType ldapType)
        {
            Array array = Array.CreateInstance(elementType, objArray.Length);
            int count = 0;
            for (int i = 0; i < objArray.Length; i++)
            {
                object? value = ConvertObject(objArray.GetValue(i), elementType, ldapType);
                if (value is null)
                {
                    continue;
                }

                array.SetValue(value, count++);
            }

            if (count < array.Length)
            {
                ResizeNonGeneric(elementType, count, ref array);
            }

            return array;
        }

        private static bool? GetBoolean(object? value)
        {
            if (value is null)
            {
                return null;
            }

            if (value is string s)
            {
                if (bool.TryParse(s, out bool boolRes))
                {
                    return boolRes;
                }
                else if (byte.TryParse(s, out byte intRes))
                {
                    return intRes switch
                    {
                        0 => false,
                        1 => true,
                        _ => null,
                    };
                }

                Debug.Fail("Invalid boolean string.");
            }

            return value switch
            {
                0 or 0L or 0u or 0d or 0m => false,
                1 or 1L or 1u or 1d or 1m => true,
                _ => null,
            };
        }
        private static byte[] GetByteArray(object? value)
        {
            return value is byte[] byteArray ? byteArray : [];
        }
        private static DateTimeOffset? GetDateTime(object? value)
        {
            if (value is not string s)
            {
                return null;
            }
            else if (DateTimeOffset.TryParseExact(s, DT_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset offset))
            {
                return offset;
            }
            else if (DateTimeOffset.TryParse(s, out DateTimeOffset fromString))
            {
                return fromString;
            }
            else
            {
                return null;
            }
        }
        private static Guid? GetGuid(object? value)
        {
            return value is byte[] byteArray && byteArray.Length == 16 ? new Guid(byteArray) : null;
        }
        private static int? GetInt32(object? value)
        {
            return value is not string s || !int.TryParse(s, out int number) ? null : (int?)number;
        }
        private static long? GetInt64(object? value)
        {
            if (value is string s && long.TryParse(s, out long longFromStr))
            {
                return longFromStr;
            }
            else if (value is byte[] bytes)
            {
                return BitConverter.ToInt64(bytes);
            }
            else
            {
                return null;
            }
            //return value is not string s || !long.TryParse(s, out long number) ? null : (long?)number;
        }
        private static string GetString(object? value)
        {
            if (value is null)
            {
                return string.Empty;
            }
            
            if (value is string s)
            {
                return s;
            }
            
            try
            {
                return Convert.ToString(value) ?? string.Empty;
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
                return string.Empty;
            }
        }

        #endregion
    }
}

