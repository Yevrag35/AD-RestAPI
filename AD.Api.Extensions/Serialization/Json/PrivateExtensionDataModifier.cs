using AD.Api.Components;
using AD.Api.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AD.Api.Serialization.Json;

/// <summary>
/// When placed on a private field or property of a type that implements <see cref="IDictionary{TKey, TValue}"/>, any
/// properties that do not have a matching member are added to that dictionary during deserialization and written during
/// serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PrivateExtensionDataAttribute : Attribute { }

/// <summary>
/// An attribute that specifies the specified base type has a private member decorated with 
/// <see cref="PrivateExtensionDataAttribute"/>.
/// </summary>
/// <remarks>
/// This is only necessary when the private member is in defined in a base type that is not is not being serialized/
/// deserialized directly. The attribute can be placed on the derived type to specify the base type with the private
/// member or on the base type itself.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class PrivateExtensionDataClassAttribute : Attribute
{
    public Type ExtensionDataClassType { get; }

    public PrivateExtensionDataClassAttribute(Type extensionDataClassType)
    {
        this.ExtensionDataClassType = extensionDataClassType;
    }
}

/// <summary>
/// A static class that provides a method to add a JSON modifier for using <see cref="PrivateExtensionDataAttribute"/>.
/// </summary>
public static class PrivateExtensionDataModifier
{
    public static void AddPrivateExtensionData(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        OneOf<FieldInfo, PropertyInfo> firstMember = GetFirstPrivateMember(typeInfo);

        if (firstMember.IsDefault)
            return;

        JsonPropertyInfo info = firstMember.Match(state: typeInfo,
            (state, field) => CreateFromField(field, state),
            (state, property) => CreateFromProperty(property, state));

        typeInfo.Properties.Add(info);
    }

    private static OneOf<FieldInfo, PropertyInfo> GetFirstPrivateMember(JsonTypeInfo typeInfo)
    {
        Type attType = typeof(PrivateExtensionDataClassAttribute);

        IEnumerable<OneOf<FieldInfo, PropertyInfo>> members;
        if (typeInfo.Type.IsDefined(attType, inherit: true))
        {
            PrivateExtensionDataClassAttribute? baseAtt = typeInfo.Type
                .GetCustomAttribute<PrivateExtensionDataClassAttribute>();

            Type baseType = ValidateClassTypeIsBaseType(baseAtt, typeInfo.Type);
            members = GetPrivateFieldAndProperties(baseType);
        }
        else
        {
            members = GetPrivateFieldAndProperties(typeInfo.Type);
        }

        OneOf<FieldInfo, PropertyInfo> firstMember = members.FirstOrDefault();
        return firstMember;
    }

    private static JsonPropertyInfo CreateFromField(FieldInfo field, JsonTypeInfo typeInfo)
    {
        JsonPropertyInfo info = typeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);

        info.IsExtensionData = true;
        info.Get = field.GetValue;
        info.Set = field.SetValue;

        return info;
    }
    private static JsonPropertyInfo CreateFromProperty(PropertyInfo property, JsonTypeInfo typeInfo)
    {
        JsonPropertyInfo info = typeInfo.CreateJsonPropertyInfo(property.PropertyType, property.Name);

        info.IsExtensionData = true;
        info.Get = property.GetValue;
        info.Set = property.SetValue;

        return info;
    }

    private static IEnumerable<OneOf<FieldInfo, PropertyInfo>> GetPrivateFieldAndProperties(Type contractType)
    {
        foreach (FieldInfo field in contractType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (IsValidPrivateMember(field, field.FieldType))
            {
                yield return field;
            }
        }

        foreach (PropertyInfo property in contractType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (property.CanWrite && IsValidPrivateMember(property, property.PropertyType))
            {
                yield return property;
            }
        }
    }

    private static bool IsValidPrivateMember<T>(T member, Type memberType) where T : MemberInfo
    {
        if (!member.IsDefined(typeof(PrivateExtensionDataAttribute)))
        {
            return false;
        }

        if (!typeof(IDictionary<string, object>).IsAssignableFrom(memberType)
            &&
            typeof(IDictionary<string, JsonElement>).IsAssignableFrom(memberType))
        {
            return false;
        }

        return true;
    }
    //private static IEnumerable<T> FilterForValidPrivateMembers<T>(IEnumerable<T> members) where T : MemberInfo
    //{
    //    foreach (T member in members.Where(x => x.IsDefined(typeof(PrivateExtensionDataAttribute))))
    //    {
    //        Type memberType = member.
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="extensionDataClassType"></param>
    /// <param name="jsonType"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static Type ValidateClassTypeIsBaseType(PrivateExtensionDataClassAttribute? attribute, Type jsonType)
    {
        if (attribute is null)
        {
            return jsonType;
        }
        else if (jsonType.Equals(attribute.ExtensionDataClassType))
        {
            return jsonType;
        }

        if (attribute.ExtensionDataClassType.IsInterface)
        {
            throw new InvalidOperationException($"The extension data class type '{attribute.ExtensionDataClassType.GetName()}' cannot be an interface and must be a base type of '{jsonType.GetName()}'.");
        }

        if (!attribute.ExtensionDataClassType.IsAssignableFrom(jsonType))
        {
            throw new InvalidOperationException($"The extension data class type '{attribute.ExtensionDataClassType.GetName()}' must be a base type from the JSON type '{jsonType.GetName()}'.");
        }

        return attribute.ExtensionDataClassType;
    }
}