using AD.Api.Components;
using System.Text.Json.Serialization.Metadata;

namespace AD.Api.Serialization.Json;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PrivateExtensionDataAttribute : Attribute { }

public static class PrivateExtensionDataModifier
{
    public static void AddPrivateExtensionData(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        OneOf<FieldInfo, PropertyInfo> firstMember = GetPrivateFieldAndProperties(typeInfo.Type)
            .FirstOrDefault();

        if (firstMember.IsDefault)
            return;

        JsonPropertyInfo info = firstMember.Match(state: typeInfo,
            (state, field) => CreateFromField(field, state),
            (state, property) => CreateFromProperty(property, state));

        typeInfo.Properties.Add(info);
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
            if (field.IsDefined(typeof(PrivateExtensionDataAttribute)))
            {
                yield return field;
            }
        }

        foreach (PropertyInfo property in contractType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (property.CanWrite && property.IsDefined(typeof(PrivateExtensionDataAttribute)))
            {
                yield return property;
            }
        }
    }
}