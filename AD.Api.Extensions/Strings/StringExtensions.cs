using AD.Api.Statics;

namespace AD.Api.Strings.Extensions
{
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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly string SPACE_STR = " ";
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

        [DebuggerStepThrough]
        public static string Format<T0, T1>(this string? value, T0 arg0, T1 arg1, IFormatProvider? provider = null)
            where T0 : notnull
            where T1 : notnull
        {
            return !string.IsNullOrEmpty(value)
                ? string.Format(
                    provider: provider,
                    format: value,
                    arg0.ToString(),
                    arg1.ToString())
                : string.Empty;
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
        public static bool IsEscapedAt(this string? value, int index, char escapeChar = CharConstants.BACKSLASH)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return IsEscapedAt(spanValue: value.AsSpan(), in index, escapeChar);
        }
        /// <summary>
        /// Determines if the character at the specified index is escaped with the specified escape character.
        /// </summary>
        /// <param name="spanValue">The span of characters where the indexed character occurs.</param>
        /// <param name="index">
        ///     The index of the character within the span where the preceding characters will be check. 
        /// </param>
        /// <param name="escapeChar">
        ///     The character that is marked as the escape character in the span.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the character at the specified index is found to be escaped; 
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsEscapedAt(this ReadOnlySpan<char> spanValue, in int index, char escapeChar = CharConstants.BACKSLASH)
        {
            int escapeCount = 0;
            // Count the number of escape characters preceding the current index.
            for (int i = index - 1; i >= 0 && escapeChar == spanValue[i]; i--)
            {
                escapeCount++;
            }

            return 0 != escapeCount % 2;
        }

        [DebuggerStepThrough]
        public static string OrEmpty(this string? value)
        {
            return value ?? string.Empty;
        }

        public static string RemoveSpaces(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            else if (!value.Contains(CharConstants.SPACE))
            {
                return value ?? string.Empty;
            }
            else if (value.Length > MAX_LENGTH)
            {
                return value.Replace(SPACE_STR, string.Empty);
            }

            ReadOnlySpan<char> chars = value.AsSpan().Trim();
            Span<char> tempChars = stackalloc char[chars.Length];
            tempChars.Fill(CharConstants.SPACE);
            int charCount = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!c.Equals(CharConstants.SPACE))
                {
                    tempChars[charCount] = c;
                    charCount++;
                }
            }

            return new string(tempChars.Trim());
        }

        [DebuggerStepThrough]
        public static string[] SplitByNewLine(this string? value, StringSplitOptions options = DEFAULT)
        {
            return !string.IsNullOrWhiteSpace(value)
                ? value.Split(Environment.NewLine, DEFAULT)
                : Array.Empty<string>();
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
}

