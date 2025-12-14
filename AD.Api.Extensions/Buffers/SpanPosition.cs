namespace AD.Api.Buffers;

/// <summary>
/// Represents a bounded region of a given span with an index and a length. This structure defines the starting index and the length of a segment within a span.
/// </summary>
[DebuggerStepThrough]
[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay(@"\{Index={Index}, Length={Length}\}")]
public readonly struct SpanPosition : IComparable<SpanPosition>, IEquatable<int>, IEquatable<SpanPosition>
{
	/// <summary>
	/// The exclusive end index of the segment within a given span.
	/// </summary>
	public readonly int End => Index + Length;
	/// <summary>
	/// The starting index of the segment within a given span.
	/// </summary>
	public readonly int Index;
	/// <summary>
	/// Indicates whether the segment is defined within a given span or represents an undefined region.
	/// </summary>
	public readonly bool IsDefined => Length > 0;
	/// <summary>
	/// The length of the segment within a given span.
	/// </summary>
	public readonly int Length;

	/// <summary>
	/// Initializes a new instance of the <see cref="SpanPosition"/> struct with the specified start index and length.
	/// </summary>
	/// <param name="start">The starting index of the span. Must be greater than or equal to 0.</param>
	/// <param name="length">The length of the span segment. Must be non-zero positive.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> is less than 0 or <paramref name="length"/> is negative.</exception>
	public SpanPosition(int start, int length)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(start);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
		Index = start;
		Length = length;
	}

	/// <summary>
	/// Compares the current <see cref="SpanPosition"/> to another <see cref="SpanPosition"/> object.
	/// </summary>
	/// <param name="other">The <see cref="SpanPosition"/> to compare with.</param>
	/// <returns>An integer that indicates the relative position in the sort order.</returns>
	public int CompareTo(SpanPosition other)
	{
		int comparison = Index.CompareTo(other.Index);
		if (comparison == 0)
		{
			comparison = Length.CompareTo(other.Length);
		}

		return comparison;
	}
	/// <summary>
	/// Determines whether the current <see cref="SpanPosition"/> is equal to the specified integer value representing a starting index.
	/// </summary>
	/// <param name="other">The integer value to compare with.</param>
	/// <returns><see langword="true"/> if the starting index is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
	public bool Equals(int other)
	{
		return Index == other;
	}
	/// <summary>
	/// Determines whether the current <see cref="SpanPosition"/> is equal to another <see cref="SpanPosition"/> object.
	/// </summary>
	/// <param name="other">The <see cref="SpanPosition"/> to compare with.</param>
	/// <returns><see langword="true"/> if both objects represent the same segment; otherwise, <see langword="false"/>.</returns>
	public bool Equals(SpanPosition other)
	{
		return Index == other.Index && Length == other.Length;
	}
	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj switch
		{
			SpanPosition pos => this.Equals(pos),
			int i => this.Equals(i),
			long longVal when longVal <= int.MaxValue && longVal >= int.MinValue => this.Equals((int)longVal),
			uint untInt when untInt <= int.MaxValue => this.Equals((int)untInt),
			ulong untLong when untLong <= int.MaxValue => this.Equals((int)untLong),
			_ => false,
		};
	}
	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return HashCode.Combine(Index, Length);
	}

	/// <summary>
	/// Converts the current <see cref="SpanPosition"/> to a <see cref="Range"/> object.
	/// </summary>
	/// <returns>A <see cref="Range"/> object representing the span position and length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Range ToRange()
	{
		return this.IsDefined ? new Range(Index, this.End) : new Range(0, 0);
	}

	/// <summary>
	/// Validates that the specified <see cref="SpanPosition"/> is within the bounds of the given length.
	/// </summary>
	/// <remarks>Use this method to ensure that a <see cref="SpanPosition"/> is valid within the context of a
	/// specified range.</remarks>
	/// <param name="position">The span position to validate, including its starting index and length.</param>
	/// <param name="length">The total length of the range against which the span position is validated.</param>
	/// <param name="paramName">The name of the parameter being validated. Automatically populated by the compiler if not explicitly provided.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="position"/> has a negative index, a negative length, or its calculated end exceeds
	/// <paramref name="length"/>.</exception>
	internal static void ThrowIfOutOfRange(SpanPosition position, int length, [CallerArgumentExpression(nameof(position))] string? paramName = null)
	{
		if (position.End > length)
		{
			throw new ArgumentOutOfRangeException(paramName, $"The span position {position} is out of range for the specified length {length}.");
		}
	}
	/// <summary>
	/// Throws an exception if the specified <see cref="SpanPosition"/> is undefined.
	/// </summary>
	/// <param name="position">The <see cref="SpanPosition"/> to validate. Must have a non-zero length.</param>
	/// <param name="paramName">The name of the parameter being validated. Automatically populated by the compiler if not explicitly provided.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="position"/> has a length of zero, indicating an undefined span position.</exception>
	public static void ThrowIfUndefined(SpanPosition position, [CallerArgumentExpression(nameof(position))] string? paramName = null)
	{
		if (0 == position.Length)
		{
			throw new ArgumentOutOfRangeException(paramName, "The span position is undefined (length is zero).");
		}
	}

	/// <summary>
	/// Represents an undefined region of a span, where the index and length are both invalid.
	/// </summary>
	public static readonly SpanPosition Undefined = default;

	/// <summary>
	/// Determines whether two <see cref="SpanPosition"/> instances are equal.
	/// </summary>
	/// <param name="left">The first <see cref="SpanPosition"/> to compare.</param>
	/// <param name="right">The second <see cref="SpanPosition"/> to compare.</param>
	/// <returns><see langword="true"/> if both <see cref="SpanPosition"/> instances are equal; otherwise, <see langword="false"/>.</returns>
	public static bool operator ==(SpanPosition left, SpanPosition right)
	{
		return left.Index == right.Index && left.Length == right.Length;
	}
	/// <summary>
	/// Determines whether two <see cref="SpanPosition"/> instances are not equal.
	/// </summary>
	/// <param name="left">The first <see cref="SpanPosition"/> to compare.</param>
	/// <param name="right">The second <see cref="SpanPosition"/> to compare.</param>
	/// <returns><see langword="true"/> if both <see cref="SpanPosition"/> instances are not equal; otherwise, <see langword="false"/>.</returns>
	public static bool operator !=(SpanPosition left, SpanPosition right)
	{
		return left.Index != right.Index || left.Length != right.Length;
	}

	public static explicit operator Range(SpanPosition position)
	{
		return position.ToRange();
	}
	public static bool operator >(SpanPosition left, SpanPosition right)
	{
		return left.CompareTo(right) > 0;
	}
	public static bool operator <(SpanPosition left, SpanPosition right)
	{
		return left.CompareTo(right) < 0;
	}
	public static bool operator >=(SpanPosition left, SpanPosition right)
	{
		return left.CompareTo(right) >= 0;
	}
	public static bool operator <=(SpanPosition left, SpanPosition right)
	{
		return left.CompareTo(right) <= 0;
	}
}