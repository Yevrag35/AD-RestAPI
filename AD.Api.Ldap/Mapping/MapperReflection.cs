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
                setAcc = pi.GetSetMethod(true);

            if (setAcc is null)
                throw new LdapMappingException(new MissingMethodException(pi.DeclaringType?.FullName, $"{pi.Name}_set"),
                    $"Property '{pi.Name}' on type '{typeof(T).Name}' has no available set accessor.");

            setAcc.Invoke(obj, new object[1] { value });
        }

        private static object? ConvertEnumerable(MemberInfo memInfo, Type toType, object? value, object? existingValue)
        {
            if (value is not Array arr || arr.Length <= 0)
                return existingValue;

            if (toType.IsArray)
            {
                return CreateArray(toType, arr, existingValue);
            }

            if (existingValue is null)
            {
                existingValue = Activator.CreateInstance(toType);
                if (existingValue is null)
                    return existingValue;
            }

            Type[] genericArguments = toType.GetGenericArguments();
            Func<object?, object?> func;
            switch (genericArguments.Length)
            {
                case 0:
                    func = (incoming) => incoming;
                    break;

                default:
                    func = (incoming) => ConvertObject(memInfo, genericArguments[0], incoming, null);
                    break;
            }

            if (!TryFindAddMethod(toType, out MethodInfo? addMethod))
                return existingValue;

            for (int i = 0; i < arr.Length; i++)
            {
                object? converted = func(arr.GetValue(i));
                ExecuteAddMethod(addMethod, existingValue, converted);
            }

            return existingValue;
        }

        private static object? ConvertObject(MemberInfo? memberInfo, Type valueType, object? rawValue, object? existingValue)
        {
            Type enumerableType = typeof(IEnumerable);
            if (memberInfo is not null && (valueType.IsArray || (!valueType.IsValueType && !valueType.Equals(typeof(string)) &&
                enumerableType.IsAssignableTo(valueType))))
            {
                return ConvertEnumerable(memberInfo, valueType, rawValue, existingValue);
            }
            else
            {
                if (rawValue is not null)
                {
                    var type = rawValue.GetType();
                    if (!type.Equals(typeof(string)) && rawValue is IEnumerable objs)
                    {
                        rawValue = objs.Cast<object>().LastOrDefault();
                    }
                }

                return Convert.ChangeType(rawValue, valueType);
            }
        }

        private static object? ConvertValue<T>([DisallowNull] T obj, MemberInfo memberInfo, LdapPropertyAttribute attribute, object? rawValue)
        {
            _ = TryGetExistingValue(obj, memberInfo, out object? existingValue, out Type valueType);

            return TryGetConverter(memberInfo, out LdapPropertyConverter? converter) && converter.CanConvert(valueType)
                ? converter.Convert(attribute, rawValue, existingValue)
                : ConvertObject(memberInfo, valueType, rawValue, existingValue);
        }

        private static object CreateArray(Type arrayType, Array list, object? existingValue)
        {
            Type? elementType = arrayType.GetElementType() ?? typeof(object);

            if (existingValue is not Array existingArray)
            {
                existingArray = Array.CreateInstance(elementType, 0);
            }

            Array arr = Array.CreateInstance(elementType, list.Length + existingArray.Length);

            if (existingArray.Length > 0)
            {
                existingArray.CopyTo(arr, 0);
            }

            for (int i = existingArray.Length; i < list.Length + existingArray.Length; i++)
            {
                object? converted = ConvertObject(null, elementType, list.GetValue(i - existingArray.Length), null);
                arr.SetValue(converted, i);
            }

            return arr;
        }

        private static void ExecuteAddMethod(MethodInfo addMethod, object collection, object? toBeAdded)
        {
            if (toBeAdded is null)
                return;

            addMethod.Invoke(collection, new object[] { toBeAdded });
        }

        private static List<(MemberInfo Member, LdapPropertyAttribute Attribute)> GetBindableMembers<T>()
        {
            List<(MemberInfo Member, LdapPropertyAttribute? Attribute)> list = new();
            IEnumerable<MemberInfo> possibles = typeof(T)
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x =>
                    x.CustomAttributes.Any(x => x.AttributeType.IsAssignableTo(typeof(LdapPropertyAttribute))));

            foreach (MemberInfo memInfo in possibles)
            {
                list.Add((memInfo, memInfo.GetCustomAttribute<LdapPropertyAttribute>()));
            }

            return list;
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
                    return false;

                converter = (LdapPropertyConverter?)Activator.CreateInstance(att.ConverterType);

                return converter is not null;
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
