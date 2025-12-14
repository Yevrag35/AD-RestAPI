namespace AD.Api.Serialization.Json;

/// <summary>
/// Represents a canonicalized classification of <see cref="JsonValueKind"/> values used by the serialization helpers.
/// </summary>
/// <remarks>
/// This sealed type provides a small set of well-known instances (<see cref="Array"/>, <see cref="Object"/>, <see cref="Other"/>)
/// that can be compared by reference. The <see langword="null"/> value is represented by the <see cref="None"/> field.
/// Reference equality is intentionally used because instances are singletons and carry no instance-specific state.
/// </remarks>
public sealed class JsonValueKindType : IEquatable<JsonValueKindType>
{
	/// <summary>
	/// Represents the <see cref="JsonValueKind.Array"/> kind.
	/// </summary>
	public static readonly JsonValueKindType Array = new(nameof(JsonValueKind.Array), JsonValueKind.Array);

	/// <summary>
	/// Represents the <see cref="JsonValueKind.Object"/> kind.
	/// </summary>
	public static readonly JsonValueKindType Object = new(nameof(JsonValueKind.Object), JsonValueKind.Object);

	/// <summary>
	/// Represents any <see cref="JsonValueKind"/> value that is not specifically mapped to a named instance.
	/// </summary>
	/// <remarks>
	/// This instance is returned for kinds such as <see cref="JsonValueKind.String"/>, <see cref="JsonValueKind.Number"/>, and others
	/// that do not have a dedicated singleton instance in this type.
	/// </remarks>
	public static readonly JsonValueKindType Other = new(string.Empty, JsonValueKind.Undefined);

	/// <summary>
	/// A convenience field that represents the absence of a <see cref="JsonValueKindType"/> value.
	/// </summary>
	/// <remarks>
	/// This field is explicitly defined as <see langword="null"/> and can be used when a nullable reference is required.
	/// </remarks>
	public static readonly JsonValueKindType? None = null;

	/// <summary>
	/// Gets the logical name associated with this instance.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the underlying <see cref="JsonValueKind"/> value represented by this instance.
	/// </summary>
	public JsonValueKind Type { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonValueKindType"/> class.
	/// </summary>
	/// <param name="name">A descriptive name for the value. The compiler constant expectation is indicated by <see cref="ConstantExpectedAttribute"/>.</param>
	/// <param name="type">The underlying <see cref="JsonValueKind"/> value.</param>
	private JsonValueKindType([ConstantExpected] string name, JsonValueKind type)
	{
		this.Name = name;
		this.Type = type;
	}

	/// <summary>
	/// Determines whether the specified <see cref="JsonValueKindType"/> is the same singleton instance as the current one.
	/// </summary>
	/// <param name="other">The other instance to compare.</param>
	/// <returns><see langword="true"/> when <paramref name="other"/> is the same reference as this instance; otherwise, <see langword="false"/>.</returns>
	public bool Equals([NotNullWhen(true)] JsonValueKindType? other)
	{
		return ReferenceEquals(this, other);
	}

	/// <summary>
	/// Determines whether the specified object reference is the same singleton instance as the current one.
	/// </summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns><see langword="true"/> when <paramref name="obj"/> is the same reference as this instance; otherwise, <see langword="false"/>.</returns>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return ReferenceEquals(this, obj);
	}

	/// <summary>
	/// Returns a hash code for this instance.
	/// </summary>
	/// <remarks>
	/// Because equality is reference-based, the hash code is the default object hash code. Do not rely on stable values across process restarts.
	/// </remarks>
	/// <returns>An <see langword="int"/> hash code for the current instance.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>
	/// Converts a <see cref="JsonValueKind"/> to the corresponding <see cref="JsonValueKindType"/> singleton or <see langword="null"/>.
	/// </summary>
	/// <param name="value">The <see cref="JsonValueKind"/> value to convert.</param>
	/// <returns>
	/// <see cref="Object"/> when <paramref name="value"/> is <see cref="JsonValueKind.Object"/>,
	/// <see cref="Array"/> when <paramref name="value"/> is <see cref="JsonValueKind.Array"/>,
	/// <see langword="null"/> when <paramref name="value"/> is <see cref="JsonValueKind.Null"/> or <see cref="JsonValueKind.Undefined"/>,
	/// otherwise <see cref="Other"/>.
	/// </returns>
	public static implicit operator JsonValueKindType?(JsonValueKind value)
	{
		return value switch
		{
			JsonValueKind.Object => Object,
			JsonValueKind.Array => Array,
			JsonValueKind.Null or JsonValueKind.Undefined => None,
			_ => Other,
		};
	}

	/// <summary>
	/// Compares two <see cref="JsonValueKindType"/> references for reference equality.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns><see langword="true"/> when both operands reference the same instance or are both <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
	public static bool operator ==(JsonValueKindType? left, JsonValueKindType? right)
	{
		return ReferenceEquals(left, right);
	}

	/// <summary>
	/// Compares two <see cref="JsonValueKindType"/> references for inequality.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns><see langword="true"/> when the operands do not reference the same instance; otherwise, <see langword="false"/>.</returns>
	public static bool operator !=(JsonValueKindType? left, JsonValueKindType? right)
	{
		return !ReferenceEquals(left, right);
	}
}
