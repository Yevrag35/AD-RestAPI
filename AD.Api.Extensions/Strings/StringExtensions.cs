namespace AD.Api.Strings.Extensions;

/// <summary>
/// An class of extension methods for <see cref="string"/> and <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public static partial class StringExtensions
{
	// SpanSplit -> StringExtensions-SpanSplit.cs

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private const string COMMA_SPACE = ", ";
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private const int MAX_LENGTH = 256;
#if NET5_0_OR_GREATER
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private const StringSplitOptions DEFAULT = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
#else
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const StringSplitOptions DEFAULT = StringSplitOptions.RemoveEmptyEntries;
#endif
	/// <summary>
	/// A string constant for a comma and space. The default sequence for splitting strings.
	/// </summary>
	/// <returns>
	///     A 2-character length <see cref="string"/> and its value <c>", "</c> without the quotes.
	/// </returns>
	public static readonly string CommaSpace = COMMA_SPACE;

	public static bool ContainsEqualAmount(this ReadOnlySpan<char> value, char open, char close)
	{
		int count = 0;
		foreach (char c in value)
		{
			if (c == open)
			{
				count++;
				continue;
			}

			if (c == close)
			{
				count--;
				continue;
			}
		}

		return count == 0;
	}

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
}

