using AD.Api.Collections.Enumerators;
using AD.Api.Core.Authentication;
using AD.Api.Statics;
using AD.Api.Validation;
using System.Collections.Immutable;

namespace AD.Api.Core.Ldap;

/// <summary>
/// Represents an LDAP distinguished name (DN) with the ability to split into its individual relative names.
/// </summary>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay(@"\{Count={Count}; Value={ToString(),nq}\}")]
public readonly partial struct DistinguishedName :
	ICanBeEmpty,
	IEnumerable<RelativeName>,
	IComparable<DistinguishedName>,
	IEquatable<DistinguishedName>,
	IEquatable<string>,
	ISpanFormattable
{
	private static readonly char COMMA = CharConstants.COMMA;
	private readonly int _length;
	private readonly bool _notDefault;
	private readonly ImmutableArray<RelativeName> _segments;

	/// <summary>
	/// An empty distinguished name containing no relative names and of zero length.
	/// </summary>
	public static readonly DistinguishedName Empty = new(default);

	/// <summary>
	/// Gets the <see cref="RelativeName"/> component at the specified index.
	/// </summary>
	/// <param name="index">
	/// The zero-based index of the <see cref="RelativeName"/> component to get.
	/// </param>
	/// <returns>
	/// The <see cref="RelativeName"/> component at the specified index.
	/// </returns>
	public ref readonly RelativeName this[int index] => ref _segments.AsSpan(index, 1)[0];

	/// <summary>
	/// Gets the number of <see cref="RelativeName"/> components in the distinguished name.
	/// </summary>
	public readonly int Count => _segments.Length;
	/// <summary>
	/// Indicates whether the distinguished name has a parent component.
	/// </summary>
	public readonly bool HasParent
	{
		[DebuggerStepThrough]
		get
		{
			ref readonly RelativeName first = ref this.GetFirst();
			return !first.IsEmpty && this.Count > 1 && first.AttributeType != RelativeNameType.DomainComponent;
		}
	}
	/// <summary>
	/// Indicates whether the distinguished name is empty or default-initialized.
	/// </summary>
	public readonly bool IsEmpty => !_notDefault || _segments.IsEmpty;
	/// <summary>
	/// Gets the <see cref="string"/> length of the entire distinguished name.
	/// </summary>
	public readonly int Length => _length;
	/// <summary>
	/// Gets the <see cref="RelativeNameType"/> of the first <see cref="RelativeName"/> component
	/// or <see cref="RelativeNameType.None"/> if empty.
	/// </summary>
	public readonly RelativeNameType Type
	{
		[DebuggerStepThrough]
		get
		{
			ref readonly RelativeName first = ref this.GetFirst();
			return first.AttributeType;
		}
	}

	private DistinguishedName(in ImmutableArray<RelativeName> segments, int length)
	{
		_notDefault = true;
		_length = length;
		_segments = segments;
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="DistinguishedName"/> struct copying the
	/// specified relative name components.
	/// </summary>
	/// <param name="segments">
	/// The relative name components to copy into the distinguished name.
	/// </param>
	public DistinguishedName(ReadOnlySpan<RelativeName> segments)
	{
		_notDefault = true;
		ImmutableArray<RelativeName> array = ImmutableArray.Create(segments);
		_segments = array;
		_length = GetTotalLength(segments);
	}

	/// <summary>
	/// Returns the distinguished name as an immutable array of relative names.
	/// </summary>
	/// <returns>An immutable array of <see cref="RelativeName"/> components.</returns>
	public readonly ImmutableArray<RelativeName> AsImmutableArray()
	{
		return !this.IsEmpty ? _segments : [];
	}
	/// <summary>
	/// Returns the distinguished name as a read-only span of the relative name components
	/// in sequence (lowest level-to-highest level).
	/// </summary>
	/// <returns>A read-only span of <see cref="RelativeName"/> components.</returns>
	public readonly ReadOnlySpan<RelativeName> AsSpan()
	{
		return _segments.AsSpan();
	}
	/// <summary>
	/// Returns the distinguished name as a read-only span of relative names starting from the specified index.
	/// </summary>
	/// <param name="start">The zero-based index to start from.</param>
	/// <returns>A read-only span of <see cref="RelativeName"/> components starting from the specified index.</returns>
	public readonly ReadOnlySpan<RelativeName> AsSpan(int start)
	{
		return !this.IsEmpty ? _segments.AsSpan(start, _segments.Length - start) : [];
	}
	/// <summary>
	/// Returns the distinguished name as a read-only span of relative names starting from the specified index
	/// and for the specified length.
	/// </summary>
	/// <param name="start">The zero-based index to start from.</param>
	/// <param name="length">The number of elements to include in the span.</param>
	/// <returns>A read-only span of <see cref="RelativeName"/> components starting from the specified index
	/// and for the specified length.</returns>
	public readonly ReadOnlySpan<RelativeName> AsSpan(int start, int length)
	{
		return !this.IsEmpty ? _segments.AsSpan(start, length) : [];
	}
	/// <summary>
	/// Copies the distinguished name to the specified span of characters.
	/// </summary>
	/// <param name="destination">The span of characters to copy to.</param>
	/// <returns>The number of characters copied.</returns>
	public readonly int CopyTo(Span<char> destination)
	{
		return CopyTo(_segments.AsSpan(), destination);
	}
	/// <summary>
	/// Copies the relative name components starting at the specified index to the specified span of characters.
	/// </summary>
	/// <param name="destination">The span of characters to copy to.</param>
	/// <param name="relativeNameIndex">The zero-based index of the relative name component to copy.</param>
	/// <returns>The number of characters copied.</returns>
	public readonly int CopyTo(Span<char> destination, int relativeNameIndex)
	{
		return this.CopyTo(destination, relativeNameIndex, this.Count - relativeNameIndex);
	}
	/// <summary>
	/// Copies the specified number of relative name components to the specified span of characters.
	/// </summary>
	/// <param name="destination">The span of characters to copy to.</param>
	/// <param name="relativeNameIndex">The zero-based index of the first relative name component to copy.</param>
	/// <param name="relativeNameLength">The number of relative name components to copy.</param>
	/// <returns>The number of characters copied.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when the length of the destination span is less than the length of the distinguished name.
	/// </exception>
	public readonly int CopyTo(Span<char> destination, int relativeNameIndex, int relativeNameLength)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, this.Length, nameof(destination));
		return CopyTo(_segments.AsSpan(relativeNameIndex, relativeNameLength), destination);
	}
	/// <summary>
	/// Returns an enumerator that iterates through the relative name components.
	/// </summary>
	/// <returns>An enumerator for the relative name components.</returns>
	public ArrayRefEnumerator<RelativeName> GetEnumerator()
	{
		ReadOnlySpan<RelativeName> span = _segments.AsSpan();
		return new ArrayRefEnumerator<RelativeName>(span);
	}
	readonly IEnumerator<RelativeName> IEnumerable<RelativeName>.GetEnumerator()
	{
		ImmutableArray<RelativeName> segs = _segments;
		return new ArrayEnumerator<RelativeName>(segs.AsSpan());
	}
	readonly IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<RelativeName>)this).GetEnumerator();
	}
	private ref readonly RelativeName GetFirst()
	{
		return ref !this.IsEmpty
			? ref _segments.AsSpan(0, 1)[0]
			: ref RelativeName.Empty;
	}

	/// <summary>
	/// Returns the relative name components of the parent distinguished name.
	/// </summary>
	/// <returns>A read-only span of the parent relative name components.</returns>
	public readonly ReadOnlySpan<RelativeName> GetParentSegments()
	{
		return this.HasParent
			? _segments.AsSpan(1, _segments.Length - 1)
			: [];
	}
	/// <summary>
	/// Returns the string representation of the parent distinguished name.
	/// </summary>
	/// <returns>The string representation of the parent distinguished name.</returns>
	public readonly string GetParent()
	{
		ReadOnlySpan<RelativeName> parentSegments = this.GetParentSegments();
		if (parentSegments.IsEmpty)
		{
			return string.Empty;
		}
		else if (parentSegments.Length == 1)
		{
			return parentSegments[0].Value;
		}

		ref readonly RelativeName first = ref this.GetFirst();
		int length = _length - first.Value.Length - 1;
		return ToString(parentSegments, length);
	}
	/// <summary>
	/// Creates a new <see cref="DistinguishedName"/> object that represents
	/// the parent path of the current distinguished name.
	/// </summary>
	/// <returns>The parent distinguished name.</returns>
	public readonly DistinguishedName ToParent()
	{
		ref readonly RelativeName first = ref this.GetFirst();
		if (first.IsEmpty || first.AttributeType == RelativeNameType.DomainComponent)
		{
			return Empty;
		}

		int length = _length - first.Value.Length - 1;
		var array = ImmutableArray.Create(_segments, 1, _segments.Length - 1);
		return new(in array, length);
	}

	/// <summary>
	/// Returns the string representation of the full distinguished name.
	/// </summary>
	/// <returns>The string representation of the full distinguished name.</returns>
	public override readonly string ToString()
	{
		return !this.IsEmpty ? ToString(_segments.AsSpan(), _length) : string.Empty;
	}
	/// <inheritdoc/>
	[DebuggerStepThrough]
	string IFormattable.ToString(string? format, System.IFormatProvider? formatProvider)
	{
		return this.ToString();
	}

	public readonly WorkingScope ToWorkingScope(ReadOnlySpan<char> domainKey, AuthorizedRole requiredRole, Span<char> buffer)
	{
		DistinguishedName @this = this;
		if (@this.IsEmpty)
		{
			return WorkingScope.Open;
		}

		int written = !this.HasParent
			? @this.CopyTo(buffer)
			: @this.CopyTo(buffer, 1);

		return new WorkingScope(domainKey, buffer.Slice(0, written), requiredRole);
	}

	/// <summary>
	/// Attempts to format the value into the provided character span.
	/// </summary>
	/// <remarks>If <paramref name="destination"/> is too small to contain the entire formatted value, no data is
	/// written and <paramref name="charsWritten"/> is set to the number of characters required.</remarks>
	/// <param name="destination">The span of characters in which to write the formatted value.</param>
	/// <param name="charsWritten">When this method returns, contains the number of characters written to <paramref name="destination"/>.</param>
	/// <returns><see langword="true"/> if the value was successfully formatted into <paramref name="destination"/>; otherwise, <see
	/// langword="false"/>.</returns>
	public bool TryFormat(Span<char> destination, out int charsWritten)
	{
		charsWritten = this.CopyTo(destination);
		return charsWritten == this.Length;
	}

	/// <inheritdoc/>
	[DebuggerStepThrough]
	bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		return this.TryFormat(destination, out charsWritten);
	}

	#region CASTING OPERATORS
	public static explicit operator DistinguishedName(string? dn)
	{
		return Parse(dn);
	}
	public static explicit operator string(DistinguishedName dn)
	{
		return dn.ToString();
	}

	#endregion

	#region STATIC METHODS
	/// <summary>
	/// Counts the number of <see cref="RelativeName"/> components in the provided span of characters if it were
	/// to be split.
	/// </summary>
	/// <param name="path">
	/// The span of characters to count the number of <see cref="RelativeName"/> components in.
	/// </param>
	/// <returns>
	/// The number of <see cref="RelativeName"/> components that would make up the distinguished name if parsed.
	/// </returns>
	public static int CountNumberOfRelativeNames(ReadOnlySpan<char> path)
	{
		if (path.IsEmpty)
		{
			return 0;
		}

		int count = 1;
		for (int i = 0; i < path.Length; i++)
		{
			if (COMMA == path[i] && !path.IsEscapedAt(i))
			{
				count++;
			}
		}

		return count;
	}
	/// <summary>
	/// Attempts to count the number of <see cref="RelativeName"/> components in the provided span of characters.
	/// </summary>
	/// <param name="path">
	/// The span of characters to count the number of <see cref="RelativeName"/> components in.
	/// </param>
	/// <param name="count">
	/// When this method returns, contains the number of <see cref="RelativeName"/> components that would make up the distinguished name if parsed.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the path contains valid relative name components; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool TryCountNumberOfRelativeNames(ReadOnlySpan<char> path, out int count)
	{
		if (path.IsEmpty)
		{
			count = 0;
			return true;
		}

		count = 1;
		for (int i = 0; i < path.Length; i++)
		{
			if (COMMA == path[i])
			{
				if (path.IsEscapedAt(i))
				{
					continue;
				}
				else if (i >= path.Length - 1)
				{
					return false;
				}

				ReadOnlySpan<char> working = path.Slice(i + 1);
				int equals = working.IndexOf(CharConstants.EQUALS);
				if (equals < 1 || !RelativeName.TryReadRelativeNameType(working.Slice(0, equals + 1), out _))
				{
					return false;
				}

				count++;
			}
		}

		return true;
	}
	/// <summary>
	/// Returns the string representation of the specified relative name components.
	/// </summary>
	/// <param name="segments">The span of relative name components to convert to a string.</param>
	/// <returns>The string representation of the specified relative name components.</returns>
	public static string ToString(ReadOnlySpan<RelativeName> segments)
	{
		if (segments.IsEmpty)
		{
			return string.Empty;
		}
		else if (segments.Length == 1)
		{
			return segments[0].Value;
		}

		int length = GetTotalLength(segments);
		return ToString(segments, length);
	}

	#endregion
}
