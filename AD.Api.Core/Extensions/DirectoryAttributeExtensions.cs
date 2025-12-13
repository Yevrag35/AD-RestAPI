using AD.Api.Core.Schema;
using System.Numerics;

namespace AD.Api.Core.Extensions;

public static class DirectoryAttributeExtensions
{
	private static readonly Type _byteArrayType = SchemaProperty.ByteArrayType;
	private static readonly Type _stringType = SchemaProperty.StringType;

	[return: NotNullIfNotNull(nameof(attribute))]
	public static byte[][]? GetAllByteArray(this DirectoryAttribute? attribute)
	{
		return attribute is not null ? GetAllByteArrayCore(attribute) : null;
	}
	public static byte[]? GetByteArray(this DirectoryAttribute? attribute)
	{
		if (attribute is null)
		{
			return null;
		}

		object[] array = attribute.GetValues(_byteArrayType);
		return array.Length > 0 && array[0] is byte[] byteArray
			? byteArray
			: [];
	}

	[return: NotNullIfNotNull(nameof(attribute))]
	public static T? GetNumber<T>(this DirectoryAttribute? attribute, [NotNullIfNotNull(nameof(attribute))] out string? strValue) where T : unmanaged, INumber<T>
	{
		strValue = GetString(attribute);
		return T.TryParse(strValue, null, out T value) ? value : default;
	}

	[return: NotNullIfNotNull(nameof(attribute))]
	public static object[]? GetObjectArray(this DirectoryAttribute? attribute, bool asByteArray = false)
	{
		if (attribute is null)
		{
			return null;
		}

		Type objectType = !asByteArray ? _stringType : _byteArrayType;
		return attribute.GetValues(objectType);
	}
	[return: NotNullIfNotNull(nameof(attribute))]
	public static string? GetString(this DirectoryAttribute? attribute)
	{
		if (attribute is null)
		{
			return null;
		}

		object[] array = attribute.GetValues(_stringType);
		return array.Length > 0 && array[0] is string str
			? str
			: string.Empty;
	}
	[return: NotNullIfNotNull(nameof(attribute))]
	public static string[]? GetStringArray(this DirectoryAttribute? attribute)
	{
		return attribute is not null ? GetStringArrayCore(attribute) : null;
	}
	[DebuggerStepThrough]
	public static bool TryGetNumber<T>(this DirectoryAttribute? attribute, out T result) where T : unmanaged, INumber<T>
	{
		return TryGetNumber(attribute, out _, out result);
	}
	public static bool TryGetNumber<T>(this DirectoryAttribute? attribute, [NotNullIfNotNull(nameof(attribute))] out string? strValue, out T result) where T : unmanaged, INumber<T>
	{
		T? value = GetNumber<T>(attribute, out strValue);
		result = value.GetValueOrDefault();
		return value.HasValue;
	}

	private static byte[][] GetAllByteArrayCore(DirectoryAttribute attribute)
	{
		object[] array = attribute.GetValues(_byteArrayType);
		return array is byte[][] byteArrayOfArray
			? byteArrayOfArray
			: [];
	}
	private static string[] GetStringArrayCore(DirectoryAttribute attribute)
	{
		object[] array = attribute.GetValues(_stringType);
		return array is string[] strArray
			? strArray
			: [];
	}
}