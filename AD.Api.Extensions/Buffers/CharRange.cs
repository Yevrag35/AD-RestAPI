using AD.Api.Validation;
using System.Buffers;
using System.Numerics;

namespace AD.Api.Buffers;

/// <summary>
/// Represents an ascending range of characters from an inclusive start character to an inclusive end character.
/// </summary>
/// <remarks>
/// An example of a <see cref="CharRange"/> would be <c>'a'..'z'</c> which represents all lowercase characters from 'a' to 'z'.
/// <para>
/// The <see cref="CharRange"/> does *NOT* hold an internal array of characters that make up the range, but still can 
/// be used to iterate through the range of characters -or- copy the characters to a provided <see cref="Span{T}"/> of
/// <see cref="char"/>, <see cref="byte"/>, or <see cref="int"/> types.
/// </para>
/// </remarks>
//[DebuggerStepThrough]
[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay(@"\{Start = {Start}, End = {End}, Length = {Length}\}")]
public readonly ref struct CharRange
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	readonly char _end;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	readonly char _start;
	/// <summary>
	/// The inclusive end character of the range.
	/// </summary>
	public readonly char End => _end;
	/// <summary>
	/// The number of characters that make up the range.
	/// </summary>
	/// <remarks>
	/// This property is calculated as <c><see cref="End"/> - <see cref="Start"/> + 1</c>.
	/// <para>Note that this value always includes <see cref="End"/> as a part of the range.</para>
	/// </remarks>
	public readonly int Length => _end - _start + 1;
	/// <summary>
	/// The inclusive start character of the range.
	/// </summary>
	public readonly char Start => _start;

	/// <inheritdoc cref="ValidateRange(int, int)" path="/exception"/>
	public CharRange(char start, char end)
	{
		ValidateRange(start, end);
		_start = start;
		_end = end;
	}

	/// <summary>
	/// Copies the range of characters to a destination <see cref="Span{T}"/> of <see cref="char"/> type.
	/// </summary>
	/// <param name="span">
	/// The destination <see cref="Span{T}"/> of <see cref="char"/> type to copy the range of characters to.
	/// </param>
	/// <inheritdoc cref="ThrowIfBufferTooSmall{T}(int, ReadOnlySpan{T}, string?)" path="/exception"/>
	public readonly void CopyTo(Span<char> span)
	{
		int length = ThrowIfBufferTooSmall(this.Length, span);
		this.CopyToCore(span, length);
	}
	/// <summary>
	/// Copies the range of characters to a destination <see cref="Span{T}"/> of <see cref="int"/> type.
	/// </summary>
	/// <param name="span">
	/// The destination <see cref="Span{T}"/> of <see cref="int"/> type to copy the range of characters to.
	/// </param>
	/// <inheritdoc cref="ThrowIfBufferTooSmall{T}(int, ReadOnlySpan{T}, string?)" path="/exception"/>
	public readonly void CopyTo(Span<int> span)
	{
		int length = ThrowIfBufferTooSmall(this.Length, span);
		this.CopyToCore(spanOfInt: span, length);
	}
	/// <summary>
	/// Copies the range of characters to a destination <see cref="Span{T}"/> of <see cref="byte"/> type.
	/// </summary>
	/// <param name="span">
	/// The destination <see cref="Span{T}"/> of <see cref="byte"/> type to copy the range of characters to.
	/// </param>
	/// <inheritdoc cref="ThrowIfBufferTooSmall{T}(int, ReadOnlySpan{T}, string?)" path="/exception"/>
	public readonly void CopyTo(Span<byte> span)
	{
		int length = ThrowIfBufferTooSmall(this.Length, span);
		this.CopyToCore(span, length);
	}
	/// <summary>
	/// Returns an enumerator that iterates through the range of characters defined by this <see cref="CharRange"/>.
	/// </summary>
	/// <remarks>
	/// The characters are returned in ascending order starting from <see cref="Start"/> up to and including <see cref="End"/>.
	/// </remarks>
	/// <returns></returns>
	public readonly Enumerator GetEnumerator()
	{
		return new Enumerator(_start, (ushort)this.Length);
	}
	/// <summary>
	/// Copies the range of characters to a new array of characters.
	/// </summary>
	/// <returns>
	/// An array of characters that contains the range of characters defined by this <see cref="CharRange"/>.
	/// </returns>
	public readonly char[] ToArray()
	{
		char[] array = new char[this.Length];
		this.CopyTo(array);
		return array;
	}

	private readonly void CopyToCore(Span<int> spanOfInt, int length)
	{
		ref int first = ref MemoryMarshal.GetReference(spanOfInt);
		for (int i = 0; i < length; i++)
		{
			Unsafe.Add(ref first, i) = _start + i;
		}
	}
	private readonly void CopyToCore<T>(Span<T> span, int length) where T : unmanaged, IMinMaxValue<T>, INumber<T>
	{
		ref T first = ref MemoryMarshal.GetReference(span);
		for (int i = 0; i < length; i++)
		{
			Unsafe.Add(ref first, i) = T.CreateChecked(_start + i);
		}
	}

	/// <summary>
	/// Creates a set of search values containing all characters in the specified inclusive range.
	/// </summary>
	/// <remarks>The returned search values can be used for efficient searching operations over spans or strings.
	/// The range must be valid; if <paramref name="end"/> is less than <paramref name="start"/>, the resulting set will be
	/// empty.</remarks>
	/// <param name="start">The first character in the range to include in the search values.</param>
	/// <param name="end">The last character in the range to include in the search values.</param>
	/// <returns>A <see cref="SearchValues{char}"/> instance containing all characters from <paramref name="start"/> to <paramref
	/// name="end"/>, inclusive.</returns>
	/// <inheritdoc cref="ValidateRange(int, int)" path="/exception"/>
	internal static SearchValues<char> CreateSearchValues(char start, char end)
	{
		const int max_stack = 256;

		CharRange range = new(start, end);
		int length = range.Length;

		using (var buffer = RentedBuffer.Rent<char>(
			length <= max_stack
				? stackalloc char[length]
				: length))
		{
			range.CopyToCore(buffer.Span, length);
			return SearchValues.Create(buffer.Span);
		}
	}

	/// <inheritdoc cref="ThrowHelper.BufferTooSmall(int, int, string?, IFormatProvider?)" path="/exception"/>
	[StackTraceHidden]
	private static int ThrowIfBufferTooSmall<T>(int rangeLength, ReadOnlySpan<T> span, [CallerArgumentExpression(nameof(span))] string? paramName = null)
		where T : unmanaged
	{
		if (span.Length < rangeLength)
		{
			ThrowHelper.BufferTooSmall(rangeLength, span.Length, paramName);
		}

		return rangeLength;
	}
	/// <exception cref="ArgumentOutOfRangeException"/>
	private static void ValidateRange(int start, int end)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(start);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(start, end);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(end, char.MaxValue);
	}

	public static implicit operator CharRange(ReadOnlySpan<char> readOnlySpan)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(readOnlySpan.Length, 2, nameof(readOnlySpan));
		char start = readOnlySpan[0];
		char end = readOnlySpan[^1];

		return end < start ? new CharRange(end, start) : new CharRange(start, end);
	}
	public static implicit operator CharRange(Span<char> span)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(span.Length, 2, nameof(span));
		char start = span[0];
		char end = span[^1];

		return end < start ? new CharRange(end, start) : new CharRange(start, end);
	}

	#region ENUMERATOR
	/// <summary>
	/// Enumerates the characters of a <see cref="CharRange"/>.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Size = 8)]
	public ref struct Enumerator
	{
		private int _index;
		private readonly char _start;
		private readonly ushort _length;

		/// <summary>
		/// Gets the element at the current position of the enumerator.
		/// </summary>
		public readonly char Current => (char)(_start + _index);

		internal Enumerator(char start, ushort length)
		{
			_start = start;
			_length = length;
			_index = -1;
		}
		/// <summary>
		/// Advances the enumerator to the next character in the range.
		/// </summary>
		/// <returns><see langword="true"/> if the enumerator was successfully advanced to the next character; 
		/// otherwise, <see langword="false"/> if the enumerator has reached the end of the range.
		/// </returns>
		public bool MoveNext()
		{
			int next = _index + 1;
			if ((uint)next >= _length)
			{
				_index = _length;
				return false;
			}

			_index = next;
			return true;
		}
		/// <summary>
		/// Resets the enumerator to its initial position, which is before the first character in the range.
		/// </summary>
		public void Reset()
		{
			_index = -1;
		}
	}

	#endregion
}