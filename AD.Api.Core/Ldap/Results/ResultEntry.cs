using AD.Api.Core.Ldap.Enums;
using AD.Api.Core.Ldap.Services.Schemas;
using AD.Api.Core.Schema;
using AD.Api.Core.Serialization;
using Microsoft.Extensions.ObjectPool;
using System.Collections;
using System.DirectoryServices.Protocols;
using System.Globalization;

namespace AD.Api.Core.Ldap.Results
{
    //[SupportedOSPlatform("WINDOWS")]
    public sealed class ResultEntry : IReadOnlyCollection<KeyValuePair<string, object>>, IResettable
    {
        private readonly SortedDictionary<string, object> _attributes;
        private readonly IAttributeConverter _converter;

        public int Count => _attributes.Count;
        public Guid LeaseId { get; set; }

        public ResultEntry(IAttributeConverter converter)
        {
            _attributes = new(StringComparer.OrdinalIgnoreCase);
            _converter = converter;
        }

        public void AddResult(string domain, SearchResultEntry entry)
        {
            _converter.ConvertEntry(domain, entry, _attributes);
        }

        //private static object? ConvertObject(DirectoryAttribute attribute, SchemaProperty property)
        //{
        //    if (attribute.Count == 0)
        //    {
        //        return null;
        //    }

        //    Type type = property.RuntimeType;
        //    if (type.IsArray)
        //    {
        //        type = type.GetElementType()!;
        //    }

        //    if (type.Equals(typeof(byte)) || type.Equals(SchemaProperty.ByteArrayType) || type.Equals(SchemaProperty.GuidType))
        //    {
        //        object[] vals = attribute.GetValues(SchemaProperty.ByteArrayType);
        //        return ConvertObject(vals[0], property.RuntimeType, property.LdapType);
        //    }
        //    else
        //    {
        //        return ConvertObject(attribute.GetValues(SchemaProperty.StringType), property.RuntimeType, property.LdapType);
        //    }
        //}
        //private static object? ConvertObject(object values, Type convertTo, LdapValueType ldapType)
        //{
        //    if (convertTo.IsArray && values is object[] objArr)
        //    {
        //        Type element = convertTo.GetElementType() ?? SchemaProperty.ObjectType;
        //        var array = Array.CreateInstance(element, objArr.Length);
        //        for (int i = 0; i < objArr.Length; i++)
        //        {
        //            array.SetValue(ConvertObject(objArr[i], element, ldapType), i);
        //        }

        //        return array;
        //    }
        //    else if (values is object[] objArr2 && objArr2.Length == 1)
        //    {
        //        return ConvertObject(objArr2[0], convertTo, ldapType);
        //    }

        //    return ldapType switch
        //    {
        //        LdapValueType.String => GetString(values),
        //        LdapValueType.ByteArray => GetByteArray(values),
        //        LdapValueType.Guid => GetGuid(values),
        //        LdapValueType.Boolean => GetBoolean(values),
        //        LdapValueType.Integer => GetInt32(values),
        //        LdapValueType.Long => GetInt64(values),
        //        LdapValueType.DateTime => GetDateTime(values),
        //        LdapValueType.Object => values,
        //        LdapValueType.StringArray => values as string[],
        //        LdapValueType.IntegerArray => values as int[],
        //        LdapValueType.LongArray => values as long[],
        //        LdapValueType.GuidArray => values as Guid[],
        //        LdapValueType.BooleanArray => values as bool[],
        //        LdapValueType.ByteTwoRankArray => values as byte[][],
        //        _ => null,
        //    };
        //}

        //private static bool? GetBoolean(object value)
        //{
        //    return value is not string s || !bool.TryParse(s, out bool boolean) ? null : (bool?)boolean;
        //}
        //private static byte[] GetByteArray(object value)
        //{
        //    return value is byte[] byteArray ? byteArray : [];
        //}
        //private static DateTimeOffset? GetDateTime(object value)
        //{
        //    if (value is not string s)
        //    {
        //        return null;
        //    }
        //    else if (DateTimeOffset.TryParse(s, out DateTimeOffset fromString))
        //    {
        //        return fromString;
        //    }
        //    else if (DateTimeOffset.TryParseExact(s, DT_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset offset))
        //    {
        //        return offset;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        //private static Guid? GetGuid(object value)
        //{
        //    return value is byte[] byteArray && byteArray.Length == 16 ? new Guid(byteArray) : null;
        //}
        //private static int? GetInt32(object value)
        //{
        //    return value is not string s || !int.TryParse(s, out int number) ? null : (int?)number;
        //}
        //private static long? GetInt64(object value)
        //{
        //    if (value is string s && long.TryParse(s, out long longFromStr))
        //    {
        //        return longFromStr;
        //    }
        //    else if (value is byte[] bytes)
        //    {
        //        return BitConverter.ToInt64(bytes);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //    //return value is not string s || !long.TryParse(s, out long number) ? null : (long?)number;
        //}
        //private static string GetString(object value)
        //{
        //    return value is string s ? s : string.Empty;
        //}

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryReset()
        {
            this.LeaseId = Guid.Empty;
            _attributes.Clear();
            return _attributes.Count == 0;
        }
    }
}

