namespace AD.Api.Reflection;

/// <summary>
/// An extension class for <see cref="Type"/> objects.
/// </summary>
public static class TypeExtensions
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="type"></param>
	/// <returns>
	/// The underlying <see cref="Type"/> if <paramref name="type"/> is nullable; 
	/// otherwise, the original <paramref name="type"/> passed.
	/// </returns>
	/// <exception cref="ArgumentNullException"/>
	public static Type GetUnderlyingType(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		return !TryGetNullableFromNonNull(type, out Type? underlying)
			? type
			: underlying;
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="underlying"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"/>
	[DebuggerStepThrough]
	public static bool TryGetNullable(this Type type, [NotNullWhen(true)] out Type? underlying)
	{
		ArgumentNullException.ThrowIfNull(type);
		return TryGetNullableFromNonNull(type, out underlying);
	}
	private static bool TryGetNullableFromNonNull(Type type, [NotNullWhen(true)] out Type? underlying)
	{
		underlying = Nullable.GetUnderlyingType(type);
		return underlying is not null;
	}
}

