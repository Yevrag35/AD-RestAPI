using AD.Api.Statics;
using AD.Api.Unmanaged;
using System.Numerics;

namespace AD.Api.Buffers;

/// <summary>
/// A ref struct that provides a way to build strings without allocating memory on the heap until the <see cref="string"/> is constructed.
/// </summary>
/// <remarks>
/// The <see cref="SpanStringBuilder"/> utilizes stack-allocated buffers and pooled arrays to avoid or reuse heap allocations during the building process.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{ToString(),nq}")]
public ref struct SpanStringBuilder
{
	internal const int DEFAULT_CAPACITY = 128;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	static readonly string s_newLine = Environment.NewLine;
	static readonly int s_newLineLength = s_newLine.Length;

	private RentedBuffer<char> _buffer;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _position;

	/// <summary>
	/// Gets a reference to the character at the specified index within the current written buffer.
	/// </summary>
	/// <param name="index">The zero-based index of the character.</param>
	/// <value>The reference to the character at the specified index.</value>
	/// <exception cref="ArgumentOutOfRangeException"/>
	public readonly ref char this[int index]
	{
		get
		{
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)_position, nameof(index));
			return ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer.Span), index);
		}
	}

	/// <summary>
	/// Gets the total capacity of the current buffer.
	/// </summary>
	/// <value>
	/// The total number of characters that the <see cref="SpanStringBuilder"/> can currently store before resizing.
	/// </value>
	public readonly int Capacity => _buffer.Length;
	/// <summary>
	/// Gets or sets a value indicating whether the buffer should be cleared when the <see cref="SpanStringBuilder"/> is disposed.
	/// </summary>
	public bool ClearOnDispose
	{
		readonly get => _buffer.ClearOnDispose;
		set => _buffer.ClearOnDispose = value;
	}
	/// <summary>
	/// Indicates whether this <see cref="SpanStringBuilder"/> instance is default-initialized.
	/// </summary>
	public readonly bool IsDefaultOrEmpty => _buffer.IsDefaultOrEmpty;
	/// <summary>
	/// Gets a value indicating whether the internal buffer is rented from the <see cref="ArrayPool{T}"/>.
	/// </summary>
	/// <value><see langword="true"/> if the buffer is rented; otherwise, <see langword="false"/>.</value>
	public readonly bool IsRented => _buffer.IsRented;
	/// <summary>
	/// Gets the number of characters appended to the <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <value>The number of characters currently in the buffer.</value>
	public readonly int Length => _position;

	/// <summary>
	/// Initializes a new instance of the <see cref="SpanStringBuilder"/> struct with a specified initial capacity.
	/// </summary>
	/// <param name="minimumCapacity">The initial capacity for the buffer.</param>
	public SpanStringBuilder(int minimumCapacity)
	{
		int capacity = Math.Max(DEFAULT_CAPACITY / 2, minimumCapacity);
		capacity = BitHelper.RoundUpToPowerOf2Unsafe(capacity);

		RentedBuffer<char> buffer = new(capacity, useEntireCapacity: true);
		_buffer = buffer;
		_position = 0;
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="SpanStringBuilder"/> struct with a pre-allocated buffer.
	/// </summary>
	/// <param name="initialBuffer">A span that provides the initial storage for the builder.</param>
	[DebuggerStepThrough]
	public SpanStringBuilder(Span<char> initialBuffer)
	{
		RentedBuffer<char> buffer = new(initialBuffer);
		_buffer = buffer;
		_position = 0;
	}

	/// <summary>
	/// Allocates a new <see cref="string"/> from the characters appended to the builder and disposes this
	/// <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <remarks>
	///     Use <see cref="ToString"/> if you want to construct additional strings from the same builder.
	/// </remarks>
	/// <returns>
	///     The constructed <see cref="string"/> instance.
	/// </returns>
	public string Build()
	{
		string str = this.ToString();
		this.Dispose();
		return str;
	}

	/// <summary>
	/// Appends a character to the <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <param name="value">The character to append.</param>
	public void Append(char value)
	{
		this.EnsureCapacity(1);
		_buffer[_position++] = value;
	}
	/// <summary>
	/// Appends a specified number of copies of the given character to the <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <param name="value">The character to append.</param>
	/// <param name="count">The number of times to append the character.</param>
	public void Append(char value, int count)
	{
		Debug.Assert(count > 0, "Count is 0 or negative!");
		count = Math.Max(0, count);
		this.EnsureCapacity(count);

		ref int pos = ref _position;
		_buffer.Slice(pos, count).Fill(value);
		pos += count;
	}
	/// <summary>
	/// Appends the string representation of the specified enumeration value to the current instance.
	/// </summary>
	/// <remarks>This method formats the enumeration value as a string and appends it to the builder.  If the
	/// enumeration value cannot be formatted within the initial buffer size, the buffer is resized dynamically.</remarks>
	/// <typeparam name="TEnum">The type of the enumeration. Must be an unmanaged enumeration type.</typeparam>
	/// <param name="enumValue">The enumeration value to append.</param>
	public void Append<TEnum>(TEnum enumValue) where TEnum : unmanaged, Enum
	{
		scoped RentedBuffer<char> buffer = new(stackalloc char[DEFAULT_CAPACITY / 2]);
		try
		{
			int written = 0;
			while (!Enum.TryFormat(enumValue, buffer.Span, out written))
			{
				int length = buffer.Length;
				buffer.Resize(length * 2);
			}

			this.Append(buffer.Slice(0, written));
		}
		finally
		{
			buffer.Dispose();
		}
	}
	/// <summary>
	/// Appends the string representation of the specified <see cref="Guid"/> to the current buffer.
	/// </summary>
	/// <remarks>The method ensures that the buffer has sufficient capacity to accommodate the appended
	/// value.</remarks>
	/// <param name="value">The <see cref="Guid"/> to append.</param>
	/// <param name="format">An optional format specifier that defines the format of the appended <see cref="Guid"/>.  If not specified, the
	/// default format is used.</param>
	/// <inheritdoc cref="LengthConstants.GetGuidLength(ReadOnlySpan{char})" path="/exception"/>
	/// <exception cref="OutOfMemoryException"/>
	public void Append(Guid value, ReadOnlySpan<char> format = default)
	{
		int length = LengthConstants.GetGuidLength(format);
		this.EnsureCapacity(length);

		var slice = _buffer.Slice(_position);
		_ = value.TryFormat(slice, out int written, format);
		_position += written;

		Debug.Assert(!slice.Slice(0, written).Contains(default), "This is where you're messing up!");
	}

	/// <summary>
	/// Appends the string representation of a specified <typeparamref name="T"/> value to this instance.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="INumber{TSelf}"/> being appended.</typeparam>
	/// <param name="number">The number to append.</param>
	/// <param name="format">The format to use.</param>
	/// <param name="provider">The format provider to use.</param>
	/// <exception cref="OutOfMemoryException"/>
	public void Append<T>(T number, IFormatProvider? provider = null) where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
	{
		int length = number.GetLength();
		this.EnsureCapacity(length);
		_position = number.CopyToSlice(_buffer.Span, _position, default, provider);
	}

	/// <summary>
	/// Appends the characters from the specified <see cref="ReadOnlySpan{T}"/> to the current instance.
	/// </summary>
	/// <param name="value">The span containing the characters to append.</param>
	public void Append(scoped ReadOnlySpan<char> value)
	{
		if (value.IsEmpty)
		{
			return;
		}

		this.EnsureCapacity(value.Length);
		_position = value.CopyToSlice(_buffer.Span, _position);
	}

	/// <summary>
	/// Appends the string representation of a specified <see cref="ISpanFormattable"/> value to this instance.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="ISpanFormattable"/> being appended.</typeparam>
	/// <param name="formattable">The formattable value to append.</param>
	/// <param name="maxLength">The maximum length of the formatted string.</param>
	/// <param name="format">An optional format string.</param>
	/// <param name="provider">An optional format provider.</param>
	public void Append<T>(T formattable, int maxLength, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
	{
		this.EnsureCapacity(maxLength);
		_position = formattable.CopyToSlice(_buffer.Span, _position, format, provider);
	}
	/// <summary>
	/// Appends the result of the provided <see cref="Action{T, TState}"/> to the current instance.
	/// </summary>
	/// <typeparam name="T">The type of state object passed to the action.</typeparam>
	/// <param name="length">The number of characters to be written.</param>
	/// <param name="state">The state passed to the <paramref name="spanAction"/>.</param>
	/// <param name="spanAction">The action that writes to the span.</param>
	public void Append<T>(int length, scoped T state, Action<Span<char>, T> spanAction) where T : allows ref struct
	{
		this.EnsureCapacity(length);
		ref int pos = ref _position;
		spanAction(_buffer.Slice(pos, length), state);
		pos += length;
	}
	/// <summary>
	/// Appends the result of the provided <see cref="Action{T, TState}"/> to the current instance.
	/// </summary>
	/// <typeparam name="T">The type of state object passed to the action.</typeparam>
	/// <param name="length">The number of characters to be written.</param>
	/// <param name="state">The state passed to the <paramref name="spanAction"/>.</param>
	/// <param name="spanAction">The action that writes to the span.</param>
	public unsafe void Append<T>(int length, scoped T state, delegate*<Span<char>, T, void> spanAction) where T : allows ref struct
	{
		this.EnsureCapacity(length);
		ref int pos = ref _position;
		spanAction(_buffer.Slice(pos, length), state);
		pos += length;
	}
	/// <summary>
	/// Appends the result of the provided delegate to the current instance.
	/// </summary>
	/// <typeparam name="T">The type of state object passed to the function.</typeparam>
	/// <param name="maxLength">The maximum number of characters that may be written.</param>
	/// <param name="state">The state passed to the <paramref name="spanFunc"/>.</param>
	/// <param name="spanFunc">The delegate that writes to the span.</param>
	public void Append<T>(int maxLength, scoped T state, Func<Span<char>, T, int> spanFunc) where T : allows ref struct
	{
		this.EnsureCapacity(maxLength);
		int written = spanFunc(_buffer.Slice(_position, maxLength), state);
		_position += written;
	}
	/// <summary>
	/// Appends a formatted value to the current buffer using a user-provided callback function.
	/// </summary>
	/// <remarks>This method ensures that the buffer has sufficient capacity to accommodate <paramref
	/// name="maxLength"/> characters before invoking the callback function. The buffer's position is updated based on the
	/// number of characters written by the callback.</remarks>
	/// <typeparam name="T">The type of the state object passed to the callback function. Must be a ref struct.</typeparam>
	/// <param name="maxLength">The maximum number of characters that can be written to the buffer. Must be a positive value.</param>
	/// <param name="state">A state object that is passed to the callback function to provide additional context or data.</param>
	/// <param name="funcPtr">A pointer to a callback function that writes the formatted value to the provided <see cref="Span{T}"/>. The
	/// function must return the number of characters written.</param>
	public unsafe void Append<T>(int maxLength, scoped T state, delegate*<Span<char>, T, int> funcPtr) where T : allows ref struct
	{
		this.EnsureCapacity(maxLength);
		var slice = _buffer.Slice(_position, maxLength);
		int written = funcPtr(slice, state);
		_position += written;

		Debug.Assert(!slice.Slice(0, written).Contains(default), "This is where you're messing up.");
	}
	internal unsafe void Append<T>(int maxLength, scoped ref T state, delegate*<Span<char>, ref T, int> funcPtr) where T : allows ref struct
	{
		this.EnsureCapacity(maxLength);
		var slice = _buffer.Slice(_position, maxLength);
		int written = funcPtr(slice, ref state);
		_position += written;

		Debug.Assert(!slice.Slice(0, written).Contains(default), "This is where you're messing up.");
	}

	/// <summary>
	/// Appends UTF-8 encoded text to the <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <param name="utf8Text">The UTF-8 encoded bytes to append.</param>
	/// <returns>The current instance after the text has been appended.</returns>
	public void Append(ReadOnlySpan<byte> utf8Text)
	{
		if (utf8Text.IsEmpty)
		{
			return;
		}

		int maxLength = Encoding.UTF8.GetMaxCharCount(utf8Text.Length);
		this.EnsureCapacity(maxLength);

		_position = utf8Text.CopyToSlice(_buffer.Span, _position);
	}
	/// <summary>
	/// Appends the specified spans of characters to the current instance.
	/// </summary>
	/// <remarks>This method allows appending multiple spans of characters in a single call.  The operation is
	/// performed efficiently using inlining to minimize overhead.</remarks>
	/// <param name="values">An array of <see cref="ReadOnlySpan{T}"/> of characters to append. Each span in the array is appended in order.</param>
	public void AppendChars(params ReadOnlySpan<char> values)
	{
		this.Append(values);
	}
	/// <summary>
	/// Appends a new line to the <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <returns>The current instance after the new line has been appended.</returns>
	public void AppendLine()
	{
		this.EnsureCapacity(s_newLineLength);
		_position = s_newLine.CopyToSlice(_buffer.Span, _position);
	}
	/// <summary>
	/// Appends a span of characters followed by a new line to the <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <param name="value">The span of characters to append.</param>
	public void AppendLine(scoped ReadOnlySpan<char> value)
	{
		this.EnsureCapacity(value.Length + s_newLineLength);

		Span<char> span = _buffer.Span;

		int pos = value.CopyToSlice(span, _position);
		_position = s_newLine.CopyToSlice(span, pos);
	}
	/// <summary>
	/// Appends a span of characters of the specified length to the current buffer.
	/// </summary>
	/// <param name="length">The number of characters to append. Must be non-negative and within the available capacity.</param>
	/// <returns>A <see cref="Span{T}"/> of characters representing the appended section of the buffer that can be written to.</returns>
	public Span<char> AppendSpan(int length)
	{
		this.EnsureCapacity(length);
		Span<char> slice = _buffer.Slice(_position, length);
		slice.Clear();

		_position += length;
		return slice;
	}

	/// <summary>
	/// Returns a span representing the current contents of the <see cref="SpanStringBuilder"/>.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> representing the characters in the builder.</returns>
	public readonly ReadOnlySpan<char> AsSpan()
	{
		return _buffer.Slice(0, _position);
	}
	/// <summary>
	/// Returns a read-only span of characters from the underlying buffer, starting at the specified position and spanning
	/// the specified length.
	/// </summary>
	/// <param name="start">The zero-based starting position of the span within the buffer.</param>
	/// <param name="length">The number of characters to include in the span.</param>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> of characters representing the specified range of the buffer.</returns>
	/// <inhertdoc cref="Span{T}.Slice(int, int)" path="/exception"/>
	public readonly ReadOnlySpan<char> AsSpan(int start, int length)
	{
		return _buffer.Slice(start, length);
	}

	/// <summary>
	/// Returns the backing <see cref="Span{T}"/> of the current buffer.
	/// </summary>
	internal readonly Span<char> GetBackingSpan()
	{
		return _buffer.Span;
	}
	/// <summary>
	/// Clears all data from the builder and resets the length to <c>0</c>.
	/// </summary>
	/// <remarks>This method removes all elements from the underlying buffer and sets the position to zero. After
	/// calling this method, the buffer will be empty, and any subsequent operations will start from the beginning of the
	/// buffer.</remarks>
	public void Clear()
	{
		_position = 0;
	}
	///// <summary>
	///// Advances the current position by the specified length and returns the updated position.
	///// </summary>
	///// <remarks>This method updates the internal position by adding the specified <paramref name="length"/> to it.
	///// Ensure that <paramref name="length"/> is non-negative to avoid exceptions.</remarks>
	///// <param name="length">The number of units to advance the position. Must be greater than or equal to 0.</param>
	///// <returns>The updated position after advancing by the specified length.</returns>
	///// <exception cref="ArgumentOutOfRangeException"/>
	//public int AdvancePosition(int length)
	//{
	//	ArgumentOutOfRangeException.ThrowIfNegative(length);
	//	Debug.Assert(length >= 0);
	//	ref int pos = ref _position;
	//	pos += length;

	//	return pos;
	//}
	/// <summary>
	/// Copies the contents of this builder to a destination span.
	/// </summary>
	/// <param name="destination">The destination span to copy the contents to.</param>
	/// <returns>
	/// The number of <see cref="char"/> elements copied to the destination span.
	/// </returns>
	public readonly int CopyTo(Span<char> destination)
	{
		_buffer.Slice(0, _position).CopyTo(destination);
		return _position;
	}

	/// <summary>
	/// Releases the resources used by the <see cref="SpanStringBuilder"/>, returning any rented buffers to the pool.
	/// </summary>
	/// <remarks>
	/// This is automatically called when invoking <see cref="Build"/>.
	/// </remarks>
	public void Dispose()
	{
		_buffer.Dispose();
		this = default;
	}
	public readonly bool EndsWith(ReadOnlySpan<char> value, StringComparison comparisonType = StringComparison.Ordinal)
	{
		return _position > 0 && _position >= value.Length
			&& _buffer.Slice(0, _position).EndsWith(value, comparisonType);
	}
	/// <summary>
	/// Returns a slice of the current buffer starting at the specified index and length.
	/// </summary>
	/// <param name="start">The starting index of the slice.</param>
	/// <param name="length">The length of the slice.</param>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> representing the specified slice.</returns>
	public readonly Span<char> GetSegment(int start, int length)
	{
		return _buffer.Slice(start, length);
	}
	/// <summary>
	/// Returns a slice of the current buffer starting at the specified starting and ending indexes.
	/// </summary>
	/// <param name="start">The starting index of the slice.</param>
	/// <param name="length">The inclusive ending index which ends the slice.</param>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> representing the specified slice.</returns>
	public readonly ReadOnlySpan<char> GetSegment(SpanPosition position)
	{
		return _buffer[position.ToRange()];
	}

	/// <summary>
	/// Finds the index of the specified character within the current buffer.
	/// </summary>
	/// <param name="value">The character to locate.</param>
	/// <returns>The zero-based index of the first occurrence of the character, or -1 if not found.</returns>
	[DebuggerStepThrough]
	public readonly int IndexOf(char value)
	{
		return _buffer.Span.IndexOf(value);
	}
	/// <summary>
	/// Finds the index of the specified character within the current buffer, starting from the specified index.
	/// </summary>
	/// <param name="value">The character to locate.</param>
	/// <param name="startIndex">The zero-based index at which to begin the search.</param>
	/// <returns>
	/// The zero-based index of the first occurrence of the character, or -1 if the character is not found.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown if <paramref name="startIndex"/> is less than 0 or greater than or equal to the length of the span.
	/// </exception>
	[DebuggerStepThrough]
	public readonly int IndexOf(char value, int startIndex)
	{
		return _buffer.Slice(startIndex).IndexOf(value);
	}
	/// <summary>
	/// Finds the index of the specified character within the current buffer, starting from the specified index and searching up to the specified count of characters.
	/// </summary>
	/// <param name="value">The character to locate.</param>
	/// <param name="startIndex">The zero-based index at which to begin the search.</param>
	/// <param name="count">The maximum number of characters to examine during the search.</param>
	/// <returns>
	/// The zero-based index of the first occurrence of the character, or -1 if the character is not found within the specified range.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown if <paramref name="startIndex"/> is less than 0 or greater than or equal to the length of the span, or if <paramref name="count"/> is less than 0 or exceeds the number of characters from <paramref name="startIndex"/> to the end of the buffer.
	/// </exception>
	[DebuggerStepThrough]
	public readonly int IndexOf(char value, int startIndex, int count)
	{
		return _buffer.Slice(startIndex, count).IndexOf(value);
	}

	/// <summary>
	/// Inserts a single character at the specified index in the buffer, shifting subsequent characters to the right.
	/// </summary>
	/// <param name="index">The zero-based index where the character will be inserted.</param>
	/// <param name="c">The character to insert.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown if <paramref name="index"/> is less than 0 or greater than the current length of the buffer.
	/// </exception>
	[DebuggerStepThrough]
	public void Insert(int index, char c)
	{
		this.Insert(index, c, 1);
	}
	/// <summary>
	/// Inserts a character at the specified index in the buffer, shifting subsequent characters to the right.
	/// </summary>
	/// <param name="index">The zero-based index where the character will be inserted.</param>
	/// <param name="c">The character to insert.</param>
	/// <param name="count">The number of times to insert the character.</param>
	public void Insert(int index, char c, int count)
	{
		this.EnsureCapacity(count);
		int remaining = _position - index;

		_buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
		_buffer.Slice(index, count).Fill(c);

		_position += count;
	}
	/// <summary>
	/// Inserts a span of characters at the specified index in the buffer, shifting subsequent characters to the right.
	/// </summary>
	/// <param name="index">The zero-based index where the characters will be inserted.</param>
	/// <param name="value">The span of characters to insert.</param>
	public void Insert(int index, scoped ReadOnlySpan<char> value)
	{
		if (value.IsEmpty)
		{
			return;
		}

		this.EnsureCapacity(value.Length);
		int remaining = _position - index;
		_buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + value.Length));
		value.CopyTo(_buffer.Slice(index));

		_position += value.Length;
	}

	public void OverlapAppend(int index, scoped ReadOnlySpan<char> value)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)_position, nameof(index));
		int newEnd = index + value.Length;               // correct end-of-data after the write
		int growth = newEnd - _position;                 // how much we extend past current end

		if (growth > 0)
			this.EnsureCapacity(growth);                      // EnsureCapacity(additionalBeyondPosition)

		value.CopyTo(_buffer.Slice(index));
		_position = newEnd; // Always overwrite the position to the new end
	}

	/// <summary>
	/// Removes the specified range of characters from the buffer.
	/// </summary>
	/// <param name="startIndex">The zero-based index at which to start removing characters.</param>
	/// <param name="length">The number of characters to remove.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="startIndex"/> is out of range for the current content
	/// or when <paramref name="length"/> exceeds the remaining characters starting at <paramref name="startIndex"/>.
	/// </exception>
	public void Remove(int startIndex, int length)
	{
		int pos = _position;

		// Fast no-op
		if ((uint)length == 0)
			return;

		// Bounds check without risk of addition overflow:
		//  - startIndex must be in [0, pos]
		//  - length must be in [0, pos - startIndex]
		if ((uint)startIndex > (uint)pos || (uint)length > (uint)(pos - startIndex))
		{
			// Match your existing parameter semantics: either start or length caused it.
			// Use the combined-message paramName if you prefer exactly the same text.
			throw new ArgumentOutOfRangeException(
				(uint)startIndex > (uint)pos ? nameof(startIndex) : nameof(length));
		}

		// Move tail left over the removed segment (CopyTo handles overlap)
		// Only the "used" slice matters; no need to touch unused capacity past _position.
		Span<char> used = _buffer.Slice(0, pos);
		used.Slice(startIndex + length).CopyTo(used.Slice(startIndex));

		_position = pos - length;
	}


	/// <summary>
	/// Allocates a new <see cref="string"/> from the characters appended to the builder.
	/// </summary>
	/// <returns>
	/// The constructed <see cref="string"/> instance.
	/// </returns>
	[DebuggerStepThrough]
	public override readonly string ToString()
	{
		return _position > 0 ? new(_buffer.Slice(0, _position)) : string.Empty;
	}

	/// <summary>
	/// Ensures that the buffer has enough capacity to accommodate the specified number of additional characters.
	/// </summary>
	/// <param name="appendLength">The number of characters to accommodate.</param>
	/// <exception cref="OutOfMemoryException">Thrown when the buffer cannot be grown further.</exception>
	private void EnsureCapacity(int appendLength)
	{
		int calculatedLength = _position + appendLength;
		if ((uint)calculatedLength > (uint)this.Capacity)
		{
			this.Grow(calculatedLength);
		}
	}
	/// <summary>
	/// Increases the capacity of the buffer to accommodate the specified minimum capacity.
	/// </summary>
	/// <param name="minimumCapacity">The minimum capacity that the buffer should have after growth.</param>
	private void Grow(int minimumCapacity)
	{
		// Ensure the capacity is a power of 2 and at least as large as minimumCapacity
		int newCapacity = Math.Max(this.Capacity * 2, minimumCapacity);
		newCapacity = BitHelper.RoundUpToPowerOf2Unsafe(newCapacity);

		Debug.Assert(newCapacity > this.Capacity, "The new capacity is smaller or equal to the current capacity.");
		Debug.Assert((newCapacity & newCapacity - 1) == 0, "The new capacity is not a power of 2."); // Ensure it's a power of 2

		_buffer.Resize(newCapacity);
	}
}