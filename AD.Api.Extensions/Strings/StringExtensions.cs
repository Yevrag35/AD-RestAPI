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

