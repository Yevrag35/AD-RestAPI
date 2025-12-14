namespace AD.Api.Strings.Extensions;

/// <summary>
/// An class of extension methods for <see cref="string"/> and <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public static partial class StringExtensions
{

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

	/// <summary>
	/// Determines whether the sequence of characters contains balanced opening and closing parentheses.
	/// </summary>
	/// <remarks>Only the '<c>(</c>' and '<c>)</c> characters are considered. Other characters are ignored. The method does not
	/// check for other types of brackets or braces.
	/// <para>
	/// If an '<c>)</c>' occurs before a opening sequence, this method will also return <see langword="false"/>. An example would be
	/// "<c>)((name=*)</c>"
	/// </para>
	/// </remarks>
	/// <param name="value">The span of characters to examine for balanced parentheses.</param>
	/// <returns>true if all opening parentheses are properly closed and nested; otherwise, false.</returns>
	public static bool IsParenthesesBalanced(this ReadOnlySpan<char> value)
	{
		uint count = 0;
		foreach (char c in value)
		{
			switch (c)
			{
				case '(':
					count++;
					break;

				case ')' when count == 0:
					return false;	// closes before an open.

				case ')':
					count--;
					break;

				default:
					break;
			}
		}

		return count == 0;
	}
}

