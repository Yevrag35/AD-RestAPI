namespace AD.Api.Reflection;

/// <summary>
/// Extension methods for <see cref="Type"/>-based classes.
/// </summary>
public static class TypeExtensions
{
	/// <summary>
	/// Returns the fully qualified name or the member name of the current <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type whose name will be returned.</param>
	/// <returns>
	/// The <see cref="Type.FullName"/> of the current <see cref="Type"/> if it is not <see langword="null"/>;
	/// otherwise, the <see cref="MemberInfo.Name"/> instead.  
	/// </returns>
	public static string GetName(this Type type)
	{
		return type.FullName ?? type.Name;
	}
	/// <summary>
	/// Returns the fully qualified name or the member name of the current <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type whose name will be returned.</param>
	/// <returns>
	/// The <see cref="Type.FullName"/> of the current <see cref="Type"/> if it is not <see langword="null"/>;
	/// otherwise, the <see cref="MemberInfo.Name"/> instead.  If the type being extended is <see langword="null"/>,
	/// then <see cref="string.Empty"/> is returned.
	/// </returns>
	public static string GetNameOrEmpty(this Type? type)
	{
		return type.GetNameOrDefault(string.Empty);
	}
	/// <summary>
	/// Returns the fully qualified name or the member name of the current <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type whose name will be returned.</param>
	/// <returns>
	/// The <see cref="Type.FullName"/> of the current <see cref="Type"/> if it is not <see langword="null"/>;
	/// otherwise, the <see cref="MemberInfo.Name"/> instead.  If the type being extended is <see langword="null"/>,
	/// then <see langword="null"/> is returned.
	/// </returns>
	[return: NotNullIfNotNull(nameof(type))]
	public static string? GetNameOrNull(this Type? type)
	{
		return type?.GetName();
	}
	/// <summary>
	/// Returns the fully qualified name or the member name of the current <see cref="Type"/> or an optional alternative value.
	/// </summary>
	/// <param name="type">The type whose name will be returned.</param>
	/// <param name="defaultValue">The alternative value to return if <paramref name="type"/> is <see langword="null"/>.</param>
	/// <returns>
	/// A string representing the name of the current <see cref="Type"/> if it is not <see langword="null"/>;
	/// otherwise, the <see cref="MemberInfo.Name"/> instead.  If the type being extended is <see langword="null"/>,
	/// is <see langword="null"/>, then <paramref name="defaultValue"/> is returned.
	/// </returns>
	[return: NotNullIfNotNull(nameof(defaultValue))]
	public static string? GetNameOrDefault(this Type? type, string? defaultValue)
	{
		return type is not null
			? type.GetName()
			: defaultValue;
	}
	/// <summary>
	/// Returns the underlying type of a nullable type, or the type itself if it is not nullable.
	/// </summary>
	/// <remarks>This method is useful for determining the non-nullable value type wrapped by a nullable type, such
	/// as <see cref="Nullable{T}"/>. If the provided type is not a nullable value type, the original type is returned
	/// unchanged.</remarks>
	/// <param name="type">The type to evaluate. If the type is a nullable value type, its underlying type is returned; otherwise, the type
	/// itself is returned. Cannot be null.</param>
	/// <returns>The underlying type if <paramref name="type"/> is a nullable value type; otherwise, <paramref name="type"/> itself.</returns>
	public static Type GetUnderlyingType(this Type type)
	{
		return TryGetNullableFromNonNullCore(type, out Type? underlying) ? underlying : type;
	}
	/// <summary>
	/// Returns the underlying non-nullable type if the specified type is a nullable value type; otherwise, returns the
	/// original type.
	/// </summary>
	/// <remarks>This method is useful for determining the non-nullable type wrapped by a nullable value type, such
	/// as <see cref="Nullable{T}"/>. If the input type is not a nullable value type, the method returns the input type
	/// itself and sets <paramref name="hasUnderlyingType"/> to <see langword="false"/>.</remarks>
	/// <param name="type">The type to inspect for an underlying non-nullable type. This must not be null.</param>
	/// <param name="hasUnderlyingType">When this method returns, contains <see langword="true"/> if the specified type is a nullable value type;
	/// otherwise, <see langword="false"/>.</param>
	/// <returns>The underlying non-nullable type if <paramref name="type"/> is a nullable value type; otherwise, the original
	/// <paramref name="type"/>.</returns>
	public static Type GetUnderlyingType(this Type type, out bool hasUnderlyingType)
	{
		Type underlying = type;
		hasUnderlyingType = false;
		if (TryGetNullableFromNonNullCore(type, out Type? nullable))
		{
			underlying = nullable;
			hasUnderlyingType = true;
		}

		return underlying;
	}
	/// <summary>
	/// Returns the underlying type of the specified nullable type, or <see langword="null"/> if the type is not a nullable type.
	/// </summary>
	/// <param name="type">The type to examine for an underlying non-nullable type.</param>
	/// <returns>The underlying non-nullable type if <paramref name="type"/> is a nullable type; otherwise, <see langword="null"/>.</returns>
	[return: NotNullIfNotNull(nameof(type))]
	public static Type? GetUnderlyingTypeOrNull(this Type? type)
	{
		return type?.GetUnderlyingType();
	}

	/// <summary>
	/// Returns the underlying type of a nullable type if the specified type is nullable; otherwise, returns the original
	/// type.
	/// </summary>
	/// <remarks>Use this method to determine whether a type is a nullable value type and to retrieve its underlying
	/// type. If the input type is not nullable, the method returns the input type itself and sets <paramref
	/// name="hasUnderlyingType"/> to <see langword="false"/>.</remarks>
	/// <param name="type">The type to examine for a nullable underlying type.</param>
	/// <param name="hasUnderlyingType">When this method returns, contains <see langword="true"/> if the specified type is nullable; otherwise, <see
	/// langword="false"/>.</param>
	/// <returns>The underlying type if <paramref name="type"/> is a nullable type; otherwise, the original type. Returns <see langword="null"/> if
	/// <paramref name="type"/> is null.</returns>
	[return: NotNullIfNotNull(nameof(type))]
	public static Type? GetUnderlyingTypeOrNull(this Type? type, out bool hasUnderlyingType)
	{
		Type? underlying = null;
		if (type is not null && !TryGetNullableFromNonNullCore(type, out underlying))
		{
			hasUnderlyingType = false;
			return type;
		}
		else
		{
			hasUnderlyingType = true;
			return underlying;
		}
	}

	/// <inheritdoc cref="Assembly.IsDefined(Type, bool)" path="/*[not(self::exception)]"/>
	/// <param name="assembly">The assembly to check for the specified attribute.</param>
	[DebuggerStepThrough]
	public static bool IsDefined<T>(this Assembly assembly) where T : Attribute
	{
		return assembly.IsDefined(typeof(T), inherit: false);
	}
	/// <inheritdoc cref="MemberInfo.IsDefined(Type, bool)"/>
	/// <param name="memberInfo">The member to check for the specified attribute.</param>
	[DebuggerStepThrough]
	public static bool IsDefined<T>(this MemberInfo memberInfo) where T : Attribute
	{
		return memberInfo.IsDefined(typeof(T), inherit: false);
	}

	/// <summary>
	/// Attempts to retrieve the underlying type argument of a nullable value type, if the specified type represents a
	/// nullable value type.
	/// </summary>
	/// <param name="type">The type to examine for a nullable value type argument. Cannot be null.</param>
	/// <param name="underlying">When this method returns <see langword="true"/>, contains the underlying type argument of the nullable value type;
	/// otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the specified type is a nullable value type and the underlying type argument was retrieved; otherwise,
	/// <see langword="false"/>.</returns>
	[DebuggerStepThrough]
	public static bool TryGetNullable(this Type type, [NotNullWhen(true)] out Type? underlying)
	{
		return TryGetNullableFromNonNullCore(type, out underlying);
	}
	private static bool TryGetNullableFromNonNullCore(Type type, [NotNullWhen(true)] out Type? underlying)
	{
		underlying = Nullable.GetUnderlyingType(type);
		return underlying is not null;
	}
}