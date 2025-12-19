using AD.Api.Buffers;

namespace AD.Api.Extensions.Strings;

/// <summary>
/// Provides extension methods for working with arrays or spans of strings.
/// </summary>
public static class MultipleStringExtensions
{
	/// <summary>
	/// Calculates the total length of all strings in the span, including the length of separators between them.
	/// </summary>
	/// <param name="strings">The span of strings to calculate the concatenated length for.</param>
	/// <param name="separatorLength">The length of the separator to include between strings. Defaults to 1.</param>
	/// <returns>The total length of the concatenated strings, including separators.</returns>
	public static int GetConcatenatedLength(this ReadOnlySpan<string?> strings, int separatorLength = 1)
	{
		Debug.Assert(separatorLength >= 0, "Separator length must be non-negative.");
		if (strings.IsEmpty)
			return 0;

		separatorLength = Math.Max(separatorLength, 0);
		ref string? first = ref MemoryMarshal.GetReference(strings);

		return GetConcatenatedLength(ref first, strings.Length, separatorLength);
	}

	private static int GetConcatenatedLength(ref string? first, int howMany, int separatorLength)
	{
		int firLength = first?.Length ?? 0;
		return (uint)howMany switch
		{
			0 => 0,
			1 => firLength,
			2 => firLength + (Unsafe.Add(ref first, 1)?.Length ?? 0) + separatorLength,
			_ => GetMoreThan2Length(ref first, firLength, howMany, separatorLength),
		};
	}

	/// <summary>
	/// Calculates the total length of all strings in the span when there are more than two strings.
	/// </summary>
	/// <param name="first">A reference to the first string in the span.</param>
	/// <param name="howMany">The total number of strings in the span.</param>
	/// <param name="separatorLength">The length of the separator to include between strings.</param>
	/// <returns>The total length of the concatenated strings, including separators.</returns>
	/// <remarks>
	/// This method iterates through the strings in the span, adding their lengths and the separator length to the total.
	/// It is used internally by <see cref="GetConcatenatedLength(ReadOnlySpan{string}, int)"/> for spans with more than two strings.
	/// </remarks>
	private static int GetMoreThan2Length(ref string? first, int firstLength, int howMany, int separatorLength)
	{
		for (int i = 1; i < howMany; i++)
		{
			firstLength += Unsafe.Add(ref first, i)?.Length ?? 0 + separatorLength;
		}

		return firstLength;
	}

	/// <summary>
	/// Concatenates the elements of the specified <see cref="ReadOnlySpan{T}"/> of strings, using the specified separator
	/// sequence between each element.
	/// </summary>
	/// <remarks>This method is optimized for performance and avoids unnecessary allocations by working directly
	/// with spans. It is suitable for scenarios where high-performance string manipulation is required.</remarks>
	/// <param name="strings">A <see cref="ReadOnlySpan{T}"/> of strings to join. If the span is empty, the method returns an empty string.</param>
	/// <param name="separator">A <see cref="ReadOnlySpan{T}"/> of characters to use as the separator. This sequence is placed between each string
	/// in the result.</param>
	/// <returns>A single string that consists of the elements in <paramref name="strings"/> delimited by the <paramref
	/// name="separator"/>. If <paramref name="strings"/> contains only one element, that element is returned without the
	/// separator.</returns>
	public static string JoinBy(this ReadOnlySpan<string?> strings, params ReadOnlySpan<char> separator)
	{
		if (strings.IsEmpty)
			return string.Empty;

		ref string? fStr = ref MemoryMarshal.GetReference(strings);
		if (strings.Length == 1)
			return fStr ?? string.Empty;

		ArgumentNullException.ThrowIfNull(fStr);
		int length = GetMoreThan2Length(ref fStr, fStr.Length, strings.Length, separator.Length);
		var tuple = RefTuple.CreateByRefOne(strings, separator, ref fStr);

		return string.Create(length, tuple, static (chars, state) =>
		{
			(var strings, var separator) = state;
			ref string? fPtr = ref state.Item3;

			fPtr.CopyTo(chars, out int written);

			for (int i = 1; i < strings.Length; i++)
			{
				written = separator.CopyToSlice(chars, written);
				written = Unsafe.Add(ref fPtr, i).CopyToSlice(chars, written);
			}

			Debug.Assert(written == chars.Length, "Written length does not match expected length.");
		});
	}

	/// <summary>
	/// Joins the elements of the specified read-only span of strings using the provided separator and writes the result to
	/// the destination character span.
	/// </summary>
	/// <remarks>If the input span is empty, no data is written and the method returns 0. The method does not
	/// allocate additional memory and writes directly to the provided destination span for performance.</remarks>
	/// <param name="strings">The read-only span of strings to join.</param>
	/// <param name="destination">The character span that receives the joined result. Must be large enough to contain the resulting string.</param>
	/// <param name="separator">A parameter array of read-only character spans used as the separator between each string.</param>
	/// <returns>The number of characters written to the destination span.</returns>
	/// <exception cref="ArgumentException">Thrown if the destination span is not large enough to hold the joined result.</exception>
	public static int JoinBy(this ReadOnlySpan<string?> strings, Span<char> destination, params ReadOnlySpan<char> separator)
	{
		if (strings.IsEmpty)
			return 0;

		ref string? fStr = ref MemoryMarshal.GetReference(strings);
		int firLength = fStr?.Length ?? 0;
		if ((strings.Length == 1 && firLength > destination.Length) || (strings.Length > 1 && GetMoreThan2Length(ref fStr, firLength, strings.Length, separator.Length) > destination.Length))
		{
			throw new ArgumentException("Destination span is not large enough to hold the joined result.", nameof(destination));
		}

		fStr.CopyTo(destination, out int written);

		for (int i = 1; i < strings.Length; i++)
		{
			written = separator.CopyToSlice(destination, written);
			written = Unsafe.Add(ref fStr, i).CopyToSlice(destination, written);
		}

		return written;
	}

	/// <summary>
	/// Concatenates the elements of the specified <see cref="ReadOnlySpan{T}"/> of strings into a UTF-8 byte span,
	/// using the specified UTF-8 byte separator between each element.
	/// </summary>
	/// <param name="strings">The <see cref="ReadOnlySpan{T}"/> of strings to join. If the span is empty, the method returns 0.</param>
	/// <param name="utf8Destination">The destination span for the resulting UTF-8 byte sequence.</param>
	/// <param name="utf8Separator">The UTF-8 byte separator to use between elements.</param>
	/// <returns>The number of bytes written to the destination span.</returns>
	public static int JoinBy(
		this ReadOnlySpan<string?> strings,
		Span<byte> utf8Destination,
		ReadOnlySpan<byte> utf8Separator)
	{
		if (strings.IsEmpty)
			return 0;

		ref string? fStr = ref MemoryMarshal.GetReference(strings);
		int written = 0;
		for (int i = 0; i < strings.Length; i++)
		{
			string? str = Unsafe.Add(ref fStr, i);
			if (str is null)
				continue;

			written += Encoding.UTF8.GetBytes(str, utf8Destination[written..]);
			if (i < strings.Length - 1)
			{
				utf8Separator.CopyTo(utf8Destination[written..]);
				written += utf8Separator.Length;
			}
		}

		return written;
	}
}