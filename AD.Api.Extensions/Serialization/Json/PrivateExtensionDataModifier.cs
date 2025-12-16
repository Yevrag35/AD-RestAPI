using AD.Api.Components;
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

		ObjEither<FieldInfo, PropertyInfo> firstMember = GetFirstPrivateMember(typeInfo);

		typeInfo.Properties.Add(firstMember.Match(state: typeInfo, fromField, fromProperty));

		static JsonPropertyInfo fromProperty(JsonTypeInfo typeInfo, PropertyInfo property)
		{
			MemberAccessor accessors = new(property);
			return CreateJsonPropertyInfo(in accessors, typeInfo);
		}

		static JsonPropertyInfo fromField(JsonTypeInfo typeInfo, FieldInfo field)
		{
			MemberAccessor accessors = new(field);
			return CreateJsonPropertyInfo(in accessors, typeInfo);
		}
	}

	private static ObjEither<FieldInfo, PropertyInfo> GetFirstPrivateMember(JsonTypeInfo typeInfo)
	{
		IEnumerable<ObjEither<FieldInfo, PropertyInfo>> members;
		if (typeInfo.Type.IsDefined(typeof(PrivateExtensionDataAttribute), inherit: true))
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

		ObjEither<FieldInfo, PropertyInfo> firstMember = members.FirstOrDefault();
		return firstMember;
	}

	private static JsonPropertyInfo CreateJsonPropertyInfo(in MemberAccessor accessors, JsonTypeInfo typeInfo)
	{
		JsonPropertyInfo info = typeInfo.CreateJsonPropertyInfo(accessors.ValueType, accessors.Name);

		info.IsExtensionData = true;
		info.Get = accessors.Getter;
		info.Set = accessors.Setter;

		return info;
	}

	private static IEnumerable<ObjEither<FieldInfo, PropertyInfo>> GetPrivateFieldAndProperties(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicPropertiesWithInherited | DynamicallyAccessedMemberTypes.NonPublicFieldsWithInherited)]
		Type contractType)
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

	private static bool IsValidPrivateMember(MemberInfo member, Type memberType)
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

	[StructLayout(LayoutKind.Auto)]
	private readonly struct MemberAccessor
	{
		public readonly Func<object, object?> Getter;
		public readonly Action<object, object?> Setter;
		public readonly string Name;
		public readonly Type ValueType;

		internal MemberAccessor(PropertyInfo property)
			: this(property, property.GetValue, property.SetValue, property.PropertyType)
		{
		}
		internal MemberAccessor(FieldInfo field)
			: this(field, field.GetValue, field.SetValue, field.FieldType)
		{
		}
		private MemberAccessor(MemberInfo member, Func<object, object?> getter, Action<object, object?> setter, Type valueType)
		{
			Name = member.Name;
			Getter = getter;
			Setter = setter;
			ValueType = valueType;
		}
	}
}