using AD.Api.Attributes;
using AD.Api.Enums.Internal;

namespace AD.Api.Enums;

/// <summary>
/// A <see langword="static"/> factory class for creating <see cref="IEnumValues{TEnum, TAtt, TValue}"/> instances.
/// </summary>
[DebuggerStepThrough]
public static class EnumValues
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <typeparam name="TAtt"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <returns></returns>
	public static IEnumValues<TEnum, TAtt, TValue> Create<TEnum, TAtt, TValue>()
		where TEnum : unmanaged, Enum
		where TAtt : Attribute, IValuedAttribute<TValue>
		where TValue : notnull
	{
		return Create<TEnum, TAtt, TValue>(freeze: false);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <typeparam name="TAtt"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="freeze"></param>
	/// <returns></returns>
	public static IEnumValues<TEnum, TAtt, TValue> Create<TEnum, TAtt, TValue>(bool freeze)
		where TEnum : unmanaged, Enum
		where TAtt : Attribute, IValuedAttribute<TValue>
		where TValue : notnull
	{
		IEnumStrings<TEnum> enumStrings = EnumStrings.Create<TEnum>(freeze);
		return Create<TEnum, TAtt, TValue>(enumStrings);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <typeparam name="TAtt"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="freeze"></param>
	/// <returns></returns>
	public static IEnumValues<TEnum, TAtt, TValue> Create<TEnum, TAtt, TValue>(IEnumStrings<TEnum> enumStrings)
		where TEnum : unmanaged, Enum
		where TAtt : Attribute, IValuedAttribute<TValue>
		where TValue : notnull
	{
		return new EVDictionary<TEnum, TAtt, TValue>(enumStrings);
	}
}

/// <summary>
/// An interface for a read-only collection of <typeparamref name="TEnum"/> values that have 
/// attributed members whose values can be read.
/// </summary>
/// <typeparam name="TEnum">The type of <see cref="Enum"/> this collection represents.</typeparam>
/// <typeparam name="TAtt">
///     The type of <see cref="Attribute"/> which implements <see cref="IValuedAttribute{TValue}"/>
///     whose values are to be read and stored.
/// </typeparam>
/// <typeparam name="TValue">
///     The return type of the <see cref="IValuedAttribute{TValue}.Value"/> property of the
///     attributed members.
/// </typeparam>
public interface IEnumValues<TEnum, TAtt, TValue> : IEnumerable<TEnum>
	where TEnum : unmanaged, Enum
	where TAtt : Attribute, IValuedAttribute<TValue>
	where TValue : notnull
{
	string this[TEnum key] { get; }

	int EnumCount { get; }
	IEnumStrings<TEnum> EnumStrings { get; }
	int ValueCount { get; }

	/// <inheritdoc cref="IEnumStrings{T}.ContainsEnum(T)"/>
	bool ContainsEnum(TEnum key);

	/// <summary>
	/// Returns an iterable collection of all <typeparamref name="TValue"/> values in this dictionary.
	/// </summary>
	/// <returns></returns>
	IEnumerable<TValue> GetAllValues();

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"/>
	TValue GetValue(TEnum key);

	[return: NotNullIfNotNull(nameof(defaultValue))]
	TValue? GetValueOrDefault(TEnum key, [AllowNull] TValue defaultValue = default);

	/// <summary>
	/// Creates a reverse lookup <see cref="Dictionary{TKey, TValue}"/> where the keys are
	/// the attribute values <typeparamref name="TValue"/> and the values are the enumeration keys 
	/// <typeparamref name="TEnum"/>.
	/// </summary>
	/// <param name="equalityComparer">
	/// The equality comparer the dictionary should use. If <see langword="null"/>, 
	/// <see cref="EqualityComparer{T}.Default"/> is used.  If <typeparamref name="TValue"/> does not implement
	/// <see cref="IEquatable{T}"/> then an <see cref="InvalidOperationException"/> is thrown.
	/// </param>
	/// <returns>
	/// A <see cref="Dictionary{TKey, TValue}"/> of <typeparamref name="TValue"/> keys and 
	/// <typeparamref name="TEnum"/> values.
	/// </returns>
	Dictionary<TValue, TEnum> ToValueDictionary(IEqualityComparer<TValue>? equalityComparer = null);

	bool TryGetAttribute(TEnum key, [NotNullWhen(true)] out TAtt? attribute);
	/// <summary>
	/// Gets the attributed value of the <typeparamref name="T"/> enumeration that is associated with the 
	/// specified key.
	/// </summary>
	/// <param name="key">
	///     The <typeparamref name="T"/> enumeration value to get the attribute value name from.
	/// </param>
	/// <param name="value">
	///     When this method returns, contains the name(s) associated with the specified value, if the
	///     name is found; otherwise, <see langword="null"/>.
	/// </param>
	/// <returns>
	///     <see langword="true"/> if the <typeparamref name="T"/> enumeration is mapped with a value;
	///     otherwise, <see langword="false"/>.
	/// </returns>
	bool TryGetValue(TEnum key, [NotNullWhen(true)] out TValue? value);
}

