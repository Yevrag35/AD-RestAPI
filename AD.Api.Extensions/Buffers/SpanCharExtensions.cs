using System.Text;

namespace AD.Api.Buffers;

/// <summary>
/// Provides extension methods for <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/> types.
/// </summary>
public static class SpanCharExtensions
{
	/// <summary>
	/// Copies the values of a <see cref="ReadOnlySpan{T}"/> to a target <see cref="Span{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of values being copied.</typeparam>
	/// <param name="values">The source span whose values are to be copied.</param>
	/// <param name="destination">The target span where values will be copied.</param>
	/// <param name="written">When this method returns, contains the number of values written to the destination span.</param>
	/// <exception cref="ArgumentException"/>
	public static void CopyTo<T>(this ReadOnlySpan<T> values, Span<T> destination, out int written)
	{
		values.CopyTo(destination);
		written = values.Length;
	}
	/// <summary>
	/// Copies the characters of a <see cref="ReadOnlySpan{T}"/> to a target <see cref="Span{T}"/>.
	/// </summary>
	/// <param name="values">The source span whose characters are to be copied.</param>
	/// <param name="destination">The target span where characters will be copied.</param>
	/// <param name="charsWritten">When this method returns, contains the number of characters written to the destination span.</param>
	/// <exception cref="ArgumentException"/>
	public static void CopyTo(this ReadOnlySpan<char> values, Span<char> destination, out int charsWritten)
	{
		values.CopyTo(destination);
		charsWritten = values.Length;
	}

	/// <summary>
	/// Copies the contents of the specified <see cref="ReadOnlySpan{T}"/> to a slice of the target <see cref="Span{T}"/>
	/// starting at the specified position.
	/// </summary>
	/// <param name="readOnlySpan">The source <see cref="ReadOnlySpan{T}"/> containing the data to copy.</param>
	/// <param name="span">The target <see cref="Span{T}"/> where the data will be copied.</param>
	/// <param name="currentPosition">The starting position in the target <paramref name="span"/> where the data will be copied.</param>
	/// <returns>The new position in the target <paramref name="span"/> after the copied data, calculated as the sum of <paramref
	/// name="currentPosition"/> and the length of <paramref name="readOnlySpan"/>.</returns>
	public static int CopyToSlice(this ReadOnlySpan<char> readOnlySpan, Span<char> span, int currentPosition)
	{
		readOnlySpan.CopyTo(span.Slice(currentPosition));
		return readOnlySpan.Length + currentPosition;
	}

	/// <summary>
	/// Formats the specified value and writes it to a slice of the provided character span.
	/// </summary>
	/// <remarks>This method attempts to format the value using the <see cref="ISpanFormattable.TryFormat"/> method.
	/// If the formatting is successful, the <paramref name="currentPosition"/> is incremented by the number of characters
	/// written.</remarks>
	/// <typeparam name="T">The type of the value to format. Must implement <see cref="ISpanFormattable"/> and be non-nullable.</typeparam>
	/// <param name="value">The value to format and write to the span.</param>
	/// <param name="span">The span of characters to which the formatted value will be written.</param>
	/// <param name="currentPosition">The starting position within the span where the formatted value will be written.  This value is updated to reflect
	/// the position after the written content.</param>
	/// <param name="format">An optional read-only span of characters that specifies the format to apply to the value.  If not provided, the
	/// default format for the type is used.</param>
	/// <param name="provider">An optional <see cref="IFormatProvider"/> to use for culture-specific formatting.  If null, the current culture is
	/// used.</param>
	/// <returns>The updated position within the span after the formatted value has been written.</returns>
	public static int CopyToSlice<T>(this T value, Span<char> span, int currentPosition, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : notnull, ISpanFormattable
	{
		if (value.TryFormat(span.Slice(currentPosition), out int written, format, provider))
		{
			currentPosition += written;
		}

		return currentPosition;
	}

	/// <summary>
	/// Copies the decoded UTF-8 text to the destination <see cref="char"/> span and returns a slice of the destination span containing the copied text.
	/// </summary>
	/// <param name="utf8Text">The UTF-8 text to decode and copy.</param>
	/// <param name="destination">The destination <see cref="char"/> span to copy the decoded text to.</param>
	/// <returns>
	/// The slice of the destination <see cref="char"/> span containing the copied text.
	/// </returns>
	public static Span<char> CopyAndSlice(this ReadOnlySpan<byte> utf8Text, Span<char> destination)
	{
		int bytesWritten = Encoding.UTF8.GetChars(utf8Text, destination);
		return destination.Slice(0, bytesWritten);
	}
	/// <summary>
	/// Copies the contents of a UTF-8 encoded <see cref="ReadOnlySpan{T}"/> of bytes into a slice of a destination <see
	/// cref="Span{T}"/> of characters, starting at the specified position.
	/// </summary>
	/// <remarks>This method decodes the UTF-8 encoded bytes in <paramref name="utf8Text"/> into characters and
	/// writes them to the <paramref name="destination"/> span starting at <paramref name="currentPosition"/>. Ensure that
	/// the destination span has sufficient space to accommodate the decoded characters; otherwise, an exception may be
	/// thrown.</remarks>
	/// <param name="utf8Text">The source span containing UTF-8 encoded bytes to be converted to characters.</param>
	/// <param name="destination">The destination span where the decoded characters will be written.</param>
	/// <param name="currentPosition">The starting position in the destination span where the characters will be written.</param>
	/// <returns>The updated position in the destination span after the characters have been written.</returns>
	public static int CopyToSlice(this ReadOnlySpan<byte> utf8Text, Span<char> destination, int currentPosition)
	{
		currentPosition += Encoding.UTF8.GetChars(utf8Text, destination.Slice(currentPosition));
		return currentPosition;
	}

	/// <summary>
	/// Determines if the character at the specified index of this <see cref="string"/> is escaped with
	/// the specified escape character.
	/// </summary>
	/// <param name="value">The string of characters where the indexed character occurs.</param>
	/// <param name="index">
	///     The index of the character within the span where the preceding characters will be check.
	/// </param>
	/// <param name="escapeChar">
	///     The character that is marked as the escape character in the span.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the character at the specified index is found to be escaped;
	/// otherwise, if it is not escaped or the <see cref="string"/> value is <see langword="null"/>, empty,
	/// or whitespace, <see langword="false"/>.
	/// </returns>
	public static bool IsEscapedAt(this ReadOnlySpan<char> value, int index, char escapeChar = '\\')
	{
		if (value.IsEmpty || index < 0 || index >= value.Length)
		{
			return false;
		}

		int i = index - 1;
		uint escapeCount = 0;

		while (i >= 0 && value[i] == escapeChar)
		{
			escapeCount++;
			i--;
		}

		return (escapeCount & 1) != 0;
	}

	/// <summary>
	/// Copies the contents of the specified spans into the target span.
	/// </summary>
	/// <remarks>This method copies the contents of the <paramref name="value"/> spans into the <paramref
	/// name="span"/> in the order they are provided. The caller must ensure that the target span has sufficient capacity
	/// to hold all the elements being copied.</remarks>
	/// <param name="span">The target <see cref="Span{T}"/> where the contents will be copied.</param>
	/// <param name="value">One or more <see cref="ReadOnlySpan{T}"/> instances containing the data to copy.</param>
	/// <returns>The total number of elements copied from the <paramref name="value"/> spans.</returns>
	public static int Push(this Span<char> span, params ReadOnlySpan<char> value)
	{
		value.CopyTo(span);
		return value.Length;
	}
	/// <summary>
	/// Attempts to copy the characters of the specified string to the provided destination span.
	/// </summary>
	/// <remarks>This method does not allocate memory. It is useful for scenarios where avoiding allocations is
	/// important, such as high-performance or low-latency applications.</remarks>
	/// <param name="value">The string to copy. If <see langword="null"/> or empty, no characters are copied, and the method returns <see
	/// langword="true"/>.</param>
	/// <param name="destination">The span to which the characters will be copied. Must have sufficient length to hold the characters of <paramref
	/// name="value"/>.</param>
	/// <param name="charsWritten">When this method returns, contains the number of characters successfully written to <paramref name="destination"/>.
	/// Set to 0 if <paramref name="value"/> is <see langword="null"/> or empty.</param>
	/// <returns><see langword="true"/> if the characters were successfully copied to <paramref name="destination"/>; otherwise,
	/// <see langword="false"/> if <paramref name="destination"/> is too small to hold the characters.</returns>
	public static bool TryCopyTo(this string? value, Span<char> destination, out int charsWritten)
	{
		charsWritten = 0;
		ReadOnlySpan<char> chars = value;
		if (chars.IsEmpty)
		{
			return true;
		}
		else if (chars.Length > destination.Length)
		{
			return false;
		}

		charsWritten = chars.Length;
		chars.CopyTo(destination);
		return true;
	}

	/// <summary>
	/// Attempts to find the zero-based index of the first occurrence of a specified character within the read-only
	/// character span.
	/// </summary>
	/// <remarks>If the character is not found, <paramref name="index"/> is set to -1.</remarks>
	/// <param name="str">The read-only span of characters to search.</param>
	/// <param name="value">The character to locate within the span.</param>
	/// <param name="index">When this method returns, contains the zero-based index of the first occurrence of <paramref name="value"/> within
	/// <paramref name="str"/>, if found; otherwise, -1.</param>
	/// <returns><see langword="true"/> if the character is found in the span; otherwise, <see langword="false"/>.</returns>
	public static bool TryIndexOf(this ReadOnlySpan<char> str, char value, out int index)
	{
		index = str.IndexOf(value);
		return index != -1;
	}
	/// <summary>
	/// Attempts to find the index of the first occurrence of a specified substring within the given string.
	/// </summary>
	/// <param name="str">The string to search within. Cannot be <see langword="null"/>.</param>
	/// <param name="value">The substring to locate within <paramref name="str"/>.</param>
	/// <param name="index">When this method returns, contains the zero-based index of the first occurrence of <paramref name="value"/> within
	/// <paramref name="str"/>, if found; otherwise, -1.</param>
	/// <returns><see langword="true"/> if <paramref name="value"/> is found within <paramref name="str"/>; otherwise, <see
	/// langword="false"/>.</returns>
	public static bool TryIndexOf(this ReadOnlySpan<char> str, [ConstantExpected] string value, out int index)
	{
		index = str.IndexOf(value);
		return index != -1;
	}

	/// <summary>
	/// Attempts to find the index of the first occurrence of a specified substring within the read-only span.
	/// </summary>
	/// <typeparam name="T">The type of elements in the span. Must be unmanaged and implement <see cref="IEquatable{T}"/>.</typeparam>
	/// <param name="span">The read-only span to search within.</param>
	/// <param name="value">The substring to locate within the span.</param>
	/// <param name="index">When this method returns, contains the zero-based index of the first occurrence of <paramref name="value"/> within
	/// <paramref name="span"/>, if found; otherwise, -1.</param>
	/// <returns><see langword="true"/> if <paramref name="value"/> is found within <paramref name="span"/>; otherwise, <see
	/// langword="false"/>.</returns>
	public static bool TryIndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, out int index) where T : unmanaged, IEquatable<T>
	{
		index = span.IndexOf(value);
		return index != -1;
	}
	/// <summary>
	/// Attempts to find the index of the first occurrence of either of the specified characters in the read-only character
	/// span.
	/// </summary>
	/// <param name="str">The read-only span of characters to search.</param>
	/// <param name="value0">The first character to search for.</param>
	/// <param name="value1">The second character to search for.</param>
	/// <param name="index">When this method returns, contains the zero-based index of the first occurrence of either character if found;
	/// otherwise, -1.</param>
	/// <returns>true if either character is found in the span; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryIndexOfAny(this ReadOnlySpan<char> str, char value0, char value1, out int index)
	{
		index = str.IndexOfAny(value0, value1);
		return index != -1;
	}

	/// <summary>
	/// Attempts to find the last occurrence of a specified character within the span.
	/// </summary>
	/// <remarks>This method does not throw an exception if the character is not found. Instead, it returns <see
	/// langword="false"/>  and sets <paramref name="index"/> to -1.</remarks>
	/// <param name="chars">The <see cref="ReadOnlySpan{T}"/> of characters to search.</param>
	/// <param name="c">The character to locate within the span.</param>
	/// <param name="index">When this method returns, contains the zero-based index of the last occurrence of the specified character,  if
	/// found; otherwise, -1.</param>
	/// <returns><see langword="true"/> if the specified character is found in the span; otherwise, <see langword="false"/>.</returns>
	public static bool TryLastIndexOf(this ReadOnlySpan<char> chars, char c, out int index)
	{
		index = chars.LastIndexOf(c);
		return index >= 0;
	}
	/// <summary>
	/// Attempts to find the last occurrence of a specified substring within the current span using the specified string
	/// comparison.
	/// </summary>
	/// <param name="chars">The span of characters to search within.</param>
	/// <param name="value">The substring to locate within the span.</param>
	/// <param name="comparisonType">The type of string comparison to use when searching for the substring.</param>
	/// <param name="index">When this method returns, contains the zero-based index of the last occurrence of <paramref name="value"/> within
	/// <paramref name="chars"/>, if found; otherwise, -1.</param>
	/// <returns><see langword="true"/> if the substring is found within the span; otherwise, <see langword="false"/>.</returns>
	public static bool TryLastIndexOf(this ReadOnlySpan<char> chars, ReadOnlySpan<char> value, StringComparison comparisonType, out int index)
	{
		index = chars.LastIndexOf(value, comparisonType);
		return index >= 0;
	}
	/// <summary>
	/// Attempts to find the last occurrence of a specified substring within the current string, using the specified
	/// comparison rules.
	/// </summary>
	/// <param name="str">The string to search within. Cannot be <see langword="null"/>.</param>
	/// <param name="value">The substring to locate within <paramref name="str"/>. Cannot be <see langword="null"/> or empty.</param>
	/// <param name="comparisonType">The type of string comparison to use, such as <see cref="StringComparison.OrdinalIgnoreCase"/>.</param>
	/// <param name="index">When this method returns, contains the zero-based index of the last occurrence of <paramref name="value"/> within
	/// <paramref name="str"/>,  or -1 if <paramref name="value"/> is not found.</param>
	/// <returns><see langword="true"/> if <paramref name="value"/> is found within <paramref name="str"/>; otherwise, <see
	/// langword="false"/>.</returns>
	public static bool TryLastIndexOf(this string str, string value, StringComparison comparisonType, out int index)
	{
		index = str.LastIndexOf(value, comparisonType);
		return index >= 0;
	}
}
