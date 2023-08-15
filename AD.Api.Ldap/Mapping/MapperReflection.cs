using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Converters;
using AD.Api.Ldap.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AD.Api.Ldap.Mapping
{
    public static partial class Mapper
    {
        private static readonly Lazy<Dictionary<Type, object>> _cachedConverters = new();

        private static void ApplyFieldValue(object obj, FieldInfo fi, object value)
        {
            fi.SetValue(obj, value);
        }

        private static void ApplyMemberValue<T>([DisallowNull] T obj, MemberInfo memberInfo, object value)
        {
            if (memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo pi)
            {
                try
                {
                    ApplyPropertyValue(obj, pi, value);
                }
                catch (Exception propEx)
                {
                    throw new LdapMappingException(propEx);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo fi)
            {
                try
                {
                    ApplyFieldValue(obj, fi, value);
                }
                catch (Exception fieldEx)
                {
                    throw new LdapMappingException(fieldEx);
                }
            }
            else
            {
                var invalid = new InvalidOperationException($"'{nameof(memberInfo)}' is not a property or field.");
                throw new LdapMappingException(invalid);
            }
        }

        private static void ApplyPropertyValue<T>([DisallowNull] T obj, PropertyInfo pi, object value)
        {
            MethodInfo? setAcc = pi.GetSetMethod();
            if (setAcc is null)
            {
                setAcc = pi.GetSetMethod(true);
            }

            if (setAcc is null)
            {
                throw new LdapMappingException(new MissingMethodException(pi.DeclaringType?.FullName, $"{pi.Name}_set"),
                    $"Property '{pi.Name}' on type '{typeof(T).Name}' has no available set accessor.");
            }

            setAcc.Invoke(obj, new object[1] { value });
        }

        private static object? ConvertEnum(object? singleObjToConvert, Type enumType)
        {
            if (!enumType.IsEnum 
                || 
                singleObjToConvert is null)
            {
                return singleObjToConvert;  // Return as is.
            }
            else if (singleObjToConvert is string stringEnumVal
                     &&
                     Enum.TryParse(enumType, stringEnumVal, out object? parsedResult))
            {
                return parsedResult;
            }

            try
            {
                return Enum.ToObject(enumType, singleObjToConvert);
            }
            catch (ArgumentException e)
            {
                throw new LdapMappingException(
                    e, 
                    $"Unable to convert object of type '{singleObjToConvert.GetType().FullName}' to enumeration of type '{enumType.FullName}'."
                );
            }
        }

        private static IEnumerable? ConvertEnumerable(MemberInfo memInfo, LdapPropertyAttribute attribute, Type toType, IEnumerable? value, IEnumerable? existingValue)
        {
            if (value is null)
            {
                return existingValue;
            }

            if (toType.IsArray)
            {
                IEnumerable? createdArray = CreateArray(toType, value);
                return createdArray is not null || existingValue is not IEnumerable arr
                    ? createdArray
                    : arr;
            }

            if (existingValue is null)
            {
                existingValue = (IEnumerable?)Activator.CreateInstance(toType);
                if (existingValue is null)
                {
                    return null;
                }
            }

            Type genericArgument = toType.GetGenericArguments().FirstOrDefault() ?? typeof(object);
            if (!TryFindAddMethod(toType, out MethodInfo? addMethod))
            {
                return existingValue;
            }

            foreach (object obj in value)
            {
                object? converted = obj;
                if (obj is not string && obj is IEnumerable eo)
                {
                    converted = ConvertObject(memInfo, attribute, genericArgument, eo, null);
                }
                else
                {
                    converted = ConvertToType(converted, genericArgument);
                }
                //converted = ConvertSingleObject(converted, genericArgument);

                ExecuteAddMethod(addMethod, existingValue, converted);
            }

            return existingValue;
        }

        private static object? ConvertObject(MemberInfo? memberInfo, LdapPropertyAttribute attribute, Type valueType, IEnumerable? rawValue, object? existingValue)
        {
            Type enumerableType = typeof(IEnumerable);
            valueType = GetNullTypeOrDefault(valueType);

            if (memberInfo is not null && (valueType.IsArray || (!valueType.IsValueType && !valueType.Equals(typeof(string)) &&
                enumerableType.IsAssignableTo(valueType))))
            {
                IEnumerable? existingCol = null;

                if (existingValue is IEnumerable isCol)
                {
                    existingCol = isCol;
                }

                return ConvertEnumerable(memberInfo, attribute, valueType, rawValue, existingCol);
            }
            else if (rawValue is not null)
            {
                IEnumerable<object> objs = rawValue.Cast<object>();
                object? single;
                if (attribute.WantsLast)
                {
                    single = objs.LastOrDefault();
                }
                else
                {
                    single = objs.ElementAt(attribute.Index);
                }

                if (!valueType.Equals(typeof(object)))
                {
                    if (valueType.IsEnum)
                    {
                        single = ConvertEnum(single, valueType);
                    }

                    single = ConvertToType(single, valueType);
                }

                return single;

                //return Convert.ChangeType(rawValue, valueType);
            }
            else
            {
                return existingValue;
            }
        }

        [Obsolete($"Use {nameof(ConvertToType)} instead.")]
        private static object? ConvertSingleObject(object? value, Type toType)
        {
            if (toType.Equals(typeof(object)))
            {
                return value;
            }

            return Convert.ChangeType(value, toType);
        }

        private static object? ConvertToType(object? singleObjToConvert, Type changeToType)
        {
            if (singleObjToConvert is null && changeToType.IsValueType)
            {
                throw new LdapMappingException(
                    new InvalidCastException(
                        $"Invalid cast from 'Null' to '{changeToType.FullName}'."
                    ), $"When the reflected property type is non-nullable, the mapped object's value cannot be null.");
            }

            try
            {
                return Convert.ChangeType(singleObjToConvert, changeToType);
            }
            catch (Exception caughtException)
            {
                Type? soType = singleObjToConvert?.GetType();

                throw new LdapMappingException(
                    caughtException, 
                    "An exception occurred when attempting to map a value of type '{0}' to type '{1}'.",
                    soType is not null
                        ? soType.FullName ?? soType.Name
                        : "null",
                    changeToType.FullName ?? changeToType.Name
                );
            }
        }

        private static object? ConvertValue<T>([DisallowNull] T obj, MemberInfo memberInfo, LdapPropertyAttribute attribute, IEnumerable? rawValue)
        {
            _ = TryGetExistingValue(obj, memberInfo, out object? existingValue, out Type valueType);

            if (rawValue is null)
            {
                return existingValue;
            }

            return TryGetConverter(memberInfo, out LdapPropertyConverter? converter) && converter.CanConvert(valueType)
                ? converter.Convert(attribute, rawValue.Cast<object>().ToArray(), existingValue)
                : ConvertObject(memberInfo, attribute, valueType, rawValue, existingValue);
        }

        private static IEnumerable? CreateArray(Type arrayType, IEnumerable? rawValue)
        {
            if (rawValue is null)
            {
                return rawValue;
            }

            Type eType = typeof(Enumerable);

            Type elementType = arrayType.GetElementType() ?? typeof(object);

            MethodInfo? castMethod = eType.GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(elementType);
            MethodInfo? toArrayMethod = eType.GetMethod(nameof(Enumerable.ToArray))?.MakeGenericMethod(elementType);

            object? castedObj = castMethod?.Invoke(null, new object[] { rawValue });
            if (castedObj is not null)
            {
                object? toArrayObj = toArrayMethod?.Invoke(null, new object[] { castedObj });
                return toArrayObj as IEnumerable;
            }

            return rawValue;
        }

        private static void ExecuteAddMethod(MethodInfo addMethod, object collection, object? toBeAdded)
        {
            if (toBeAdded is null)
            {
                return;
            }

            addMethod.Invoke(collection, new object[] { toBeAdded });
        }

        private static List<(MemberInfo Member, LdapPropertyAttribute Attribute)> GetBindableMembers<T>()
        {
            List<(MemberInfo Member, LdapPropertyAttribute Attribute)> list = new();
            IEnumerable<MemberInfo> possibles = typeof(T)
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x =>
                    x.CustomAttributes.Any(x => x.AttributeType.IsAssignableTo(typeof(LdapPropertyAttribute))));

            foreach (MemberInfo memInfo in possibles)
            {
                LdapPropertyAttribute attribute = memInfo.GetCustomAttribute<LdapPropertyAttribute>()
                    ?? throw new LdapMappingException($"Unable to retrieve the {nameof(LdapPropertyAttribute)} on the member named '{memInfo.Name}'.");

                list.Add((memInfo, attribute));
            }

            return list;
        }

        private static Type GetNullTypeOrDefault(Type valueType)
        {
            Type? tempType = Nullable.GetUnderlyingType(valueType);

            return tempType is null
                ? valueType
                : tempType;
        }

        private static bool TryFindAddMethod(Type collectionType, [NotNullWhen(true)] out MethodInfo? addMethod)
        {
            addMethod = null;

            Type[] parameterTypes = collectionType.GenericTypeArguments.Length <= 0
                ? (new Type[1] { typeof(object) })
                : collectionType.GenericTypeArguments;

            try
            {
                addMethod = collectionType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, parameterTypes);
                return addMethod is not null;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetConverter(MemberInfo memberInfo, [NotNullWhen(true)] out LdapPropertyConverter? converter)
        {
            converter = null;

            if (memberInfo.CustomAttributes.Any(x => x.AttributeType.IsAssignableTo(typeof(LdapConverterAttribute))))
            {
                LdapConverterAttribute? att = memberInfo.GetCustomAttribute<LdapConverterAttribute>();
                if (att is null)
                {
                    return false;
                }

                if (_cachedConverters.IsValueCreated && _cachedConverters.Value.TryGetValue(att.ConverterType, out object? cachedConverter))
                {
                    converter = (LdapPropertyConverter?)cachedConverter;

                    return converter is not null;
                }
                else
                {
                    converter = (LdapPropertyConverter?)Activator.CreateInstance(att.ConverterType);

                    if (converter is not null)
                    {
                        _cachedConverters.Value.Add(att.ConverterType, converter);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryGetExistingValue([NotNull] object fromObj, MemberInfo memberInfo,
            [NotNullWhen(true)] out object? existingValue, out Type memberValueType)
        {
            memberValueType = typeof(object);
            existingValue = null;

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    PropertyInfo pi = (PropertyInfo)memberInfo;
                    existingValue = pi.GetValue(fromObj);
                    memberValueType = pi.PropertyType;
                    break;

                case MemberTypes.Field:
                    FieldInfo fi = (FieldInfo)memberInfo;
                    existingValue = fi.GetValue(fromObj);
                    memberValueType = fi.FieldType;
                    break;

                default:
                    break;
            }

            return existingValue is not null;
        }
    }
}
