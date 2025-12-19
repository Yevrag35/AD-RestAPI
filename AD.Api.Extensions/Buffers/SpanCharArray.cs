using AD.Api.Validation;
using System.Collections.Immutable;

namespace AD.Api.Buffers;

/// <summary>
/// A ref struct that manages segments of character spans within a <see cref="SpanStringBuilder"/>. 
/// It allows adding, removing, and accessing segments of characters as if they were in an array, 
/// while minimizing heap allocations by using <see cref="ArrayPool{T}"/> for buffer management.
/// </summary>
/// <remarks>
/// The purpose of <see cref="SpanCharArray"/> is to efficiently manage a collection of character spans 
/// that represent segments within a <see cref="SpanStringBuilder"/>. It provides functionality for 
/// adding, removing, and splitting segments without heap allocations, making it well-suited for 
/// performance-sensitive operations.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("Count = {_index} ({_builder})")]
public ref struct SpanCharArray
{
	/// <summary>
	/// The multiple used to align capacity increments. 
	/// </summary>
	/// <remarks>
	/// This value helps in adjusting the internal buffer size to multiples of 16.
	/// </remarks>
	const int MULTIPLE = 16;
	/// <summary>
	/// The offset subtracted from the next capacity alignment.
	/// </summary>
	/// <remarks>
	/// This is always <c>MULTIPLE - 1</c>, ensuring alignment to multiples of <see cref="MULTIPLE"/>.
	/// </remarks>
	const int INCREMENT = MULTIPLE - 1;
	/// <summary>
	/// The default separator character used when none is explicitly provided.
	/// </summary>
	/// <value>
	/// One space: <c>" "</c>
	/// </value>
	private static ReadOnlySpan<char> _defaultSeparator => [' '];

	private int _index;

	private SpanStringBuilder _builder;
	private Span<SpanPosition> _positions;
	private RentedBuffer<SpanPosition> _positionBuffer;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)] private ReadOnlySpan<char> _separator;

	/// <summary>
	/// Gets the read-only span at the specified index.
	/// </summary>
	/// <param name="index">The index within the array to use.</param>
	/// <returns>The <see cref="ReadOnlySpan{T}"/> at the specified index.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or greater than or equal to <see cref="Count"/>.</exception>
	public readonly ReadOnlySpan<char> this[int index]
	{
		get
		{
			ThrowHelper.ThrowIfNegativeOrGreaterThanOrEqualTo(index, (uint)_positionBuffer.Length);
			return this.GetSegmentCore(index);
		}
	}

	/// <summary>
	/// Gets the total capacity of the backing array for segments.
	/// </summary>
	/// <value>The total capacity of the backing buffer used to store span segments.</value>
	public readonly int Capacity => _positions.Length;
	/// <summary>
	/// Gets the number of segments currently in the <see cref="SpanCharArray"/>.
	/// </summary>
	/// <value>The number of segments stored in the array.</value>
	public readonly int Count => _index;
	/// <summary>
	/// Gets the number of characters that make up the entire array including separators.
	/// </summary>
	public readonly int Length => _builder.Length;
	/// <summary>
	/// Indicates whether this <see cref="SpanCharArray"/> instance is empty or is default-initialized.
	/// </summary>
	public readonly bool IsDefaultOrEmpty => _positionBuffer.IsDefaultOrEmpty || _builder.IsDefaultOrEmpty;
	/// <summary>
	/// Gets a value indicating whether any of the internal buffers are rented from the <see cref="ArrayPool{T}"/>.
	/// </summary>
	/// <value><see langword="true"/> if the buffer is rented; otherwise, <see langword="false"/>.</value>
	public readonly bool IsRented => _positionBuffer.IsRented || _builder.IsRented;
	/// <summary>
	/// Gets the separator used to separate segments in the backing array builder and in the output <see cref="string"/>.
	/// </summary>
	public readonly ReadOnlySpan<char> Separator => _separator;

	/// <summary>
	/// Initializes a new instance of the <see cref="SpanCharArray"/> struct with a specified minimum capacity of the underlying buffer.
	/// </summary>
	/// <param name="minimumCapacity">The minimum capacity of the underlying buffer.</param>
	public SpanCharArray(int minimumCapacity) : this(minimumCapacity, minimumNumberOfPositions: 16, _defaultSeparator)
	{
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="SpanCharArray"/> struct with a specified minimum length of the array and a minimum capacity of the underlying buffer.
	/// </summary>
	/// <param name="minimumBuilderCapacity">The minimum capacity of the underlying buffer.</param>
	/// <param name="minimumNumberOfPositions">The minimum length for the underlying array buffer.</param>
	/// <param name="separator">The characters that separate each segment in the backing array builder.</param>
	public SpanCharArray(int minimumBuilderCapacity, int minimumNumberOfPositions, ReadOnlySpan<char> separator)
	{
		int capacity = BitHelper.RoundUpToPowerOf2(Math.Max(
			SpanStringBuilder.DEFAULT_CAPACITY,
			minimumBuilderCapacity));

		var buffer = RentedBuffer.Rent<SpanPosition>(minimumNumberOfPositions, useEntireCapacity: true);
		_positions = buffer.Span;
		_positionBuffer = buffer;
		_index = 0;
		_builder = new(capacity);
		_separator = separator;
	}
	public SpanCharArray(Span<char> builderBuffer, Span<SpanPosition> positions, ReadOnlySpan<char> separator)
	{
		_builder = new(builderBuffer);
		_positionBuffer = new RentedBuffer<SpanPosition>(positions);
		_positions = positions;
		_index = 0;
		_separator = separator;
	}

	/// <summary>
	/// Adds a segment of characters to the <see cref="SpanCharArray"/>.
	/// </summary>
	/// <param name="value">The span of characters to add.</param>
	/// <returns>The current instance after the segment has been added.</returns>
	[DebuggerStepThrough]
	public void Add(ReadOnlySpan<char> value)
	{
#if DEBUG || DEBUG_NOSAVE
		int previousCount = this.Count;
#endif
		this.EnsureCapacity(1);

		this.AddSegment(value);

#if DEBUG || DEBUG_NOSAVE
		Debug.Assert(this.Count == 1 + previousCount);
#endif
	}

	/// <summary>
	/// Adds multiple segments of characters by splitting the input span based on a specified separator.
	/// </summary>
	/// <param name="value">The span of characters to split and add as segments.</param>
	/// <param name="splitBy">The separator used to split the characters.</param>
	public void AddRange(ReadOnlySpan<char> value, params ReadOnlySpan<char> splitBy)
	{
#if DEBUG || DEBUG_NOSAVE
		int previousCount = this.Count;
#endif
		int count = value.Count(splitBy) + 1;
		this.EnsureCapacity(count);

		foreach (Range section in value.Split(splitBy))
		{
			this.AddSegment(value[section]);
		}

#if DEBUG || DEBUG_NOSAVE
		Debug.Assert(this.Count == count + previousCount);
#endif
	}
	/// <summary>
	/// Adds a segment of characters to the <see cref="SpanCharArray"/>.
	/// </summary>
	/// <param name="value">The span of characters to add.</param>
	[DebuggerStepThrough]
	public void AddScoped(scoped ReadOnlySpan<char> value)
	{
#if DEBUG || DEBUG_NOSAVE
		int previousCount = this.Count;
#endif
		this.EnsureCapacity(1);

		this.AddSegment(value);

#if DEBUG || DEBUG_NOSAVE
		Debug.Assert(this.Count == 1 + previousCount);
#endif
	}

	public void AddUnsafe([ConstantExpected] string value, int start)
	{
		Debug.Assert(!string.IsNullOrEmpty(value));
		Debug.Assert(start >= 0 && start < value.Length);

		this.EnsureCapacity(1);
		unsafe
		{
			fixed (char* pStr = value)
			{
				this.AddSegment(new ReadOnlySpan<char>(pStr + start, value.Length - start));
			}
		}
	}
	/// <summary>
	/// Appends a span of characters to the builder and returns a span representing the added characters.
	/// </summary>
	/// <remarks>If the builder is not empty, a separator is added before the new span. The returned span excludes
	/// the separator.</remarks>
	/// <param name="length">The number of characters to append to the builder.</param>
	/// <returns>A <see cref="Span{T}"/> of characters representing the newly added span. The span will be of the specified length.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="length"/> is negative.</exception>
	public Span<char> AddSpan(int length)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(length);

		bool noSpace = _index == 0;
		int currentLength = _builder.Length;
		if (!noSpace)
		{
			currentLength += _separator.Length;
		}

		_positions[_index++] = new(currentLength, length);
		Span<char> slice;
		if (noSpace)
		{
			slice = _builder.AppendSpan(length);
			slice.Clear();
			return slice;
		}

		slice = _builder.AppendSpan(length + _separator.Length);
		_separator.CopyTo(slice);

		slice = slice[_separator.Length..];
		slice.Clear();

		return slice;
	}

	/// <summary>
	/// Adds a segment of characters to the underlying <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <param name="value">The span of characters to add.</param>
	/// <remarks>
	/// This is a private helper method. If needed, a separator is appended 
	/// before adding <paramref name="value"/> when it's not the first entry.
	/// </remarks>
	private void AddSegment(scoped ReadOnlySpan<char> value)
	{
		bool noSpace = _index == 0;
		int length = _builder.Length;
		if (!noSpace)
		{
			length += _separator.Length;
		}

		SpanPosition pos = new(length, value.Length);
		_positions[_index++] = pos;
		if (noSpace)
		{
			_builder.Append(value);
			return;
		}

		Span<char> writeTo = _builder.AppendSpan(value.Length + _separator.Length);

		_separator.CopyTo(writeTo);
		value.CopyTo(writeTo.Slice(_separator.Length));
	}
	/// <summary>
	/// Adds a segment of characters to the underlying <see cref="SpanStringBuilder"/>, optionally trimming the segment.
	/// </summary>
	/// <param name="value">The span of characters to add.</param>
	/// <param name="trimValue">
	/// <see langword="true"/> to trim <paramref name="value"/> before adding;
	/// <see langword="false"/> to add the span as-is.
	/// </param>
	/// <remarks>
	/// If trimming results in an empty span, the segment is not added.
	/// </remarks>
	private void AddSegment(ReadOnlySpan<char> value, bool trimValue)
	{
		if (trimValue)
		{
			value = value.Trim();
			if (value.IsEmpty)
				return;
		}

		this.AddSegment(value);
	}

	/// <summary>
	/// Returns a <see cref="ReadOnlySpan{T}"/> representing the entire contents of the underlying <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> of characters representing all the segments added to the builder separated .</returns>
	public readonly ReadOnlySpan<char> AsSpan()
	{
		return _builder.AsSpan();
	}
	/// <summary>
	/// Determines whether the <see cref="SpanCharArray"/> contains the specified value, 
	/// using a case-insensitive comparison by default.
	/// </summary>
	/// <param name="value">The span of characters to search for.</param>
	/// <returns>
	/// <see langword="true"/> if <paramref name="value"/> is found in any segment; 
	/// otherwise, <see langword="false"/>.
	/// </returns>
	[DebuggerStepThrough]
	public readonly bool Contains(ReadOnlySpan<char> value)
	{
		return this.Contains(value, StringComparison.OrdinalIgnoreCase);
	}
	/// <summary>
	/// Determines whether the <see cref="SpanCharArray"/> contains the specified value, 
	/// using a specified <see cref="StringComparison"/>.
	/// </summary>
	/// <param name="value">The span of characters to search for.</param>
	/// <param name="comparison">The <see cref="StringComparison"/> to use.</param>
	/// <returns>
	/// <see langword="true"/> if <paramref name="value"/> is found in any segment; 
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public readonly bool Contains(ReadOnlySpan<char> value, StringComparison comparison)
	{
		if (value.IsEmpty || this.Count == 0)
		{
			return false;
		}

		for (int i = 0; i < _index; i++)
		{
			if (this.GetSegmentCore(i).Equals(value, comparison))
			{
				return true;
			}
		}

		return false;
	}
	/// <summary>
	/// Determines whether the underlying span contains any of the specified characters.
	/// </summary>
	/// <remarks>
	/// Checks if any character in <paramref name="values"/> is found in the underlying 
	/// <see cref="SpanStringBuilder"/>'s span. Returns <see langword="false"/> if this 
	/// <see cref="SpanCharArray"/> is default/empty.
	/// <para>
	/// Unlike <see cref="Contains(ReadOnlySpan{char})"/>, which checks for an exact match of a segment, this method looks for the presence of 
	/// any subset of characters within the entire constructed span - even across segment boundaries. To search only within individual segments, 
	/// use <see cref="ContainsAnyInSegment(SearchValues{char})"/>.
	/// </para>
	/// </remarks>
	/// <param name="values">A <see cref="SearchValues{T}"/> collection of characters to search for.</param>
	/// <returns>
	/// <see langword="true"/> if at least one character from <paramref name="values"/> is found; 
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public readonly bool ContainsAny(SearchValues<char> values)
	{
		return !this.IsDefaultOrEmpty && _builder.AsSpan().ContainsAny(values);
	}
	/// <summary>
	/// Determines whether the underlying span contains any of the specified strings.
	/// </summary>
	/// <remarks>
	/// Checks if any string in <paramref name="values"/> is found in the underlying 
	/// <see cref="SpanStringBuilder"/>'s span. Returns <see langword="false"/> if this 
	/// <see cref="SpanCharArray"/> is default/empty.
	/// <para>
	/// Unlike <see cref="Contains(ReadOnlySpan{char})"/>, which checks for an exact match of a segment, this method looks for the presence of 
	/// any subset of characters within the entire constructed span - even across segment boundaries. To search only within individual segments, 
	/// use <see cref="ContainsAnyInSegment(SearchValues{string})"/>.
	/// </para>
	/// </remarks>
	/// <param name="values">A <see cref="SearchValues{T}"/> collection of strings to search for.</param>
	/// <returns>
	/// <see langword="true"/> if at least one string from <paramref name="values"/> is found; 
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public readonly bool ContainsAny(SearchValues<string> values)
	{
		return !this.IsDefaultOrEmpty && _builder.AsSpan().ContainsAny(values);
	}
	/// <summary>
	/// Determines whether any character in the specified set appears in any segment of the current <see cref="SpanCharArray"/>.
	/// </summary>
	/// <remarks>If the current instance is uninitialized or contains no segments, the method returns
	/// false.</remarks>
	/// <param name="values">A set of characters to search for within the segments.</param>
	/// <returns><see langword="true"/> if at least one character from the specified set is found in any segment; otherwise, <see langword="false"/>.</returns>"
	public readonly bool ContainsAnyInSegment(SearchValues<char> values)
	{
		if (this.IsDefaultOrEmpty)
			return false;

		for (int i = 0; i < _index; i++)
		{
			if (this.GetSegmentCore(i).ContainsAny(values))
			{
				return true;
			}
		}

		return false;
	}
	/// <summary>
	/// Determines whether any substring in the specified set appears in any segment of the current <see cref="SpanCharArray"/>.
	/// </summary>
	/// <remarks>If the current instance is uninitialized or contains no segments, the method returns
	/// false.</remarks>
	/// <param name="values">A set of characters to search for within the segments.</param>
	/// <returns><see langword="true"/> if at least one substring from the specified set is found in any segment; otherwise, <see langword="false"/>.</returns>
	public readonly bool ContainsAnyInSegment(SearchValues<string> values)
	{
		if (this.IsDefaultOrEmpty)
			return false;

		for (int i = 0; i < _index; i++)
		{
			if (this.GetSegmentCore(i).ContainsAny(values))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Allocates a new <see cref="string"/> from the segments added and disposes this <see cref="SpanCharArray"/>.
	/// </summary>
	/// <remarks>
	/// Use <see cref="ToString"/> if you want to construct additional strings from the same array.
	/// </remarks>
	/// <returns>The constructed <see cref="string"/> instance.</returns>
	[DebuggerStepThrough]
	public string Build()
	{
		string result = this.ToString();
		this.Dispose();
		return result;
	}
	/// <summary>
	/// Releases the resources used by the <see cref="SpanCharArray"/>, returning any rented buffers to the pool.
	/// </summary>
	public void Dispose()
	{
		SpanStringBuilder builder = _builder;
		RentedBuffer<SpanPosition> array = _positionBuffer;

		this = default;

		builder.Dispose();
		array.Dispose();
	}

	/// <summary>
	/// Removes the first occurrence of the specified span of characters from the <see cref="SpanCharArray"/>.
	/// </summary>
	/// <param name="value">The span of characters to remove.</param>
	/// <returns><see langword="true"/> if the segment was successfully removed; otherwise, <see langword="false"/>.</returns>
	[DebuggerStepThrough]
	public bool Remove(ReadOnlySpan<char> value)
	{
		return this.Remove(value, StringComparison.OrdinalIgnoreCase);
	}
	/// <summary>
	/// Removes the first occurrence of the specified span of characters from the <see cref="SpanCharArray"/>, using a specific string comparison.
	/// </summary>
	/// <param name="value">The span of characters to remove.</param>
	/// <param name="comparison">The string comparison to use.</param>
	/// <returns><see langword="true"/> if the segment was successfully removed; otherwise, <see langword="false"/>.</returns>
	public bool Remove(ReadOnlySpan<char> value, StringComparison comparison)
	{
		bool removed = false;
		for (int i = 0; i < _index; i++)
		{
			SpanPosition pos = _positionBuffer[i];
			if (_builder.GetSegment(pos).Equals(value, comparison))
			{
				this.RemoveAtCore(i, pos);

				removed = true;
				break;
			}
		}

		return removed;
	}
	/// <summary>
	/// Removes the segment at the specified index from the <see cref="SpanCharArray"/>.
	/// </summary>
	/// <param name="index">The index of the segment to remove.</param>
	/// <exception cref="ArgumentOutOfRangeException"/>
	public void RemoveAt(int index)
	{
		ThrowHelper.ThrowIfNegativeOrGreaterThanOrEqualTo(index, (uint)_positionBuffer.Length);
		this.RemoveAtCore(index, _positionBuffer[index]);
	}
	/// <summary>
	/// Removes the segment at the specified index from the <see cref="SpanCharArray"/>.
	/// </summary>
	/// <param name="index">The index of the segment to remove.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown if <paramref name="index"/> is less than 0 or greater than the current length.
	/// </exception>
	private void RemoveAtCore(int index, SpanPosition pos)
	{
		int sepLength = _separator.Length;
		int lengthToRemove = pos.Index + pos.Length < _builder.Length
			? pos.Length + sepLength
			: pos.Length;

		_builder.Remove(pos.Index, lengthToRemove);

		Span<SpanPosition> allPos = _positions;
		Span<SpanPosition> posAtIndex = allPos.Slice(index);
		int minusOne = posAtIndex.Length - 1;

		for (int i = 0; i < posAtIndex.Length; i++)
		{
			if (i < minusOne)
			{
				posAtIndex[i] = posAtIndex[i + 1];
			}
			else
			{
				posAtIndex[i] = default;
			}
		}

		_positions = allPos.Slice(0, allPos.Length - 1);
	}

	/// <summary>
	/// Allocates a new <see cref="ImmutableArray{T}"/> of <see cref="string"/> from the character spans added to this <see cref="SpanCharArray"/>.
	/// </summary>
	/// <returns>
	/// A new <see cref="ImmutableArray{T}"/> of <see cref="string"/> instances with a length equal to <see cref="Count"/>
	/// containing the segments allocated as strings that have been added to this <see cref="SpanCharArray"/>. If no segments have 
	/// been added, an empty array is returned.
	/// </returns>
	public readonly ImmutableArray<string> ToImmutableStringArray()
	{
		string[] array = this.ToStringArray();
		return array.Length > 0
			? [.. array]
			: [];
	}
	/// <summary>
	/// Allocates a new <see cref="string"/> array from the character spans added to this <see cref="SpanCharArray"/>.
	/// </summary>
	/// <returns>
	/// A new <see cref="string"/> array with a length equal to <see cref="Count"/>
	/// containing the segments allocated as strings that have been added to this <see cref="SpanCharArray"/>. If no segments have 
	/// been added, an empty array is returned.
	/// </returns>
	public readonly string[] ToStringArray()
	{
		int count = _index;
		if ((uint)count == 0)
			return [];

		string[] array = new string[count];

		for (int i = 0; i < count; i++)
		{
			array[i] = new string(this.GetSegmentCore(i));
		}

		return array;
	}
	/// <summary>
	/// Allocates a new <see cref="string"/> from the character spans added to this <see cref="SpanCharArray"/>.
	/// </summary>
	/// <returns>The constructed <see cref="string"/> instance.</returns>
	[DebuggerStepThrough]
	public override readonly string ToString()
	{
		return _builder.ToString();
	}

	/// <summary>
	/// Returns a new <see cref="SpanCharArray"/> by splitting the input span using the specified separator 
	/// (which is reused for constructing the final segments).
	/// </summary>
	/// <param name="value">The span of characters to split.</param>
	/// <param name="splitBy">The separator used to split the characters.</param>
	/// <returns>A <see cref="SpanCharArray"/> containing the split segments.</returns>
	[DebuggerStepThrough]
	public static SpanCharArray Split(ReadOnlySpan<char> value, ReadOnlySpan<char> splitBy)
	{
		return Split(value, splitBy, splitBy);
	}
	/// <summary>
	/// Returns a new <see cref="SpanCharArray"/> by splitting the input span using the specified separator.
	/// </summary>
	/// <param name="value">The span of characters to split.</param>
	/// <param name="splitBy">The separator used to split the characters.</param>
	/// <param name="separator">The separator used when constructed back the array of segments.</param>
	/// <returns>The constructed <see cref="SpanCharArray"/> instance.</returns>
	public static SpanCharArray Split(ReadOnlySpan<char> value, ReadOnlySpan<char> separator, params ReadOnlySpan<char> splitBy)
	{
		if (value.IsWhiteSpace())
		{
			return new(SpanStringBuilder.DEFAULT_CAPACITY, MULTIPLE, separator);
		}

		int count = value.Count(splitBy) + 1;
		SpanCharArray array = new(value.Length + separator.Length * count, count, separator);

		foreach (Range section in value.Split(splitBy))
		{
			array.AddSegment(value[section]);
		}

		return array;
	}
	/// <summary>
	/// Returns a new <see cref="SpanCharArray"/> by splitting the input span at any character 
	/// in <paramref name="splitByAny"/>, storing <paramref name="separator"/> between segments 
	/// in the final builder. Entries can optionally be trimmed.
	/// </summary>
	/// <param name="value">The span of characters to split.</param>
	/// <param name="separator">The separator used when reconstructing the segments in the builder.</param>
	/// <param name="splitByAny">
	/// A <see cref="SearchValues{T}"/> describing which characters should cause splits.
	/// </param>
	/// <param name="trimEntries">
	/// <see langword="true"/> to trim each segment before adding; 
	/// <see langword="false"/> to keep them as-is.
	/// </param>
	/// <returns>
	/// A <see cref="SpanCharArray"/> containing the split segments. 
	/// </returns>
	public static SpanCharArray Split(ReadOnlySpan<char> value, ReadOnlySpan<char> separator, SearchValues<char> splitByAny, bool trimEntries = true)
	{
		SpanCharArray array;
		if (!value.ContainsAny(splitByAny))
		{
			array = new SpanCharArray(SpanStringBuilder.DEFAULT_CAPACITY, MULTIPLE, separator);
			array.AddSegment(value, trimEntries);

			return array;
		}
		else
		{
			array = new(value.Length * 2, (int)Math.Ceiling(value.Length / 2d), separator);
		}

		foreach (Range range in value.SplitAny(splitByAny))
		{
			array.AddSegment(value[range], trimEntries);
		}

		return array;
	}

	/// <summary>
	/// Aligns the <paramref name="capacity"/> to the nearest multiple of <c>MULTIPLE</c>.
	/// </summary>
	/// <param name="capacity">The capacity to align.</param>
	/// <returns>An adjusted capacity that is a multiple of <see cref="MULTIPLE"/>.</returns>
	[DebuggerStepThrough]
	private static int AdjustCapacity(int capacity)
	{
		return capacity + INCREMENT & ~INCREMENT;
	}
	/// <summary>
	/// Ensures that the buffer has enough capacity to accommodate the specified number of additional segments.
	/// </summary>
	/// <param name="appendLength">The number of segments to accommodate.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="appendLength"/> is negative.</exception>
	[DebuggerStepThrough]
	private void EnsureCapacity(int appendLength)
	{
		Debug.Assert(appendLength >= 0, "Append length must be non-negative.");
		int calculatedLength = _index + appendLength;
		if (calculatedLength > this.Capacity)
		{
			this.Grow(calculatedLength);
		}
	}
	/// <summary>
	/// Retrieves the <see cref="ReadOnlySpan{T}"/> of characters for the segment at the specified index.
	/// </summary>
	/// <param name="index">The index of the segment.</param>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> referencing that segment.</returns>
	[DebuggerStepThrough]
	private readonly ReadOnlySpan<char> GetSegmentCore(int index)
	{
		SpanPosition position = Unsafe.Add(ref MemoryMarshal.GetReference(_positionBuffer.Span), index);
		return _builder.GetSegment(position);
	}
	/// <summary>
	/// Grows the internal buffer to accommodate a specified minimum capacity.
	/// </summary>
	/// <param name="minimumCapacity">The minimum capacity that the buffer should accommodate.</param>
	[DebuggerStepThrough]
	private void Grow(int minimumCapacity)
	{
		Debug.Assert(minimumCapacity >= this.Capacity);
		int newCapacity = AdjustCapacity(minimumCapacity);
		Debug.Assert(newCapacity % MULTIPLE == 0);

		newCapacity = _positionBuffer.Resize(newCapacity);
		_positions = _positionBuffer.Slice(0, newCapacity);
	}
}