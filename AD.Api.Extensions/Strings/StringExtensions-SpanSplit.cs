using AD.Api.Strings.Spans;

namespace AD.Api.Strings.Extensions
{
    public static partial class StringExtensions
    {
        /// <summary>
        /// Splits the characters in the span based on a single separator character.
        /// </summary>
        /// <param name="span">The span of characters to split.</param>
        /// <param name="splitBy">The character to use as a separator.</param>
        /// <returns>A <see cref="SplitEnumerator"/> for enumerating over the split entries.</returns>
        [DebuggerStepThrough]
        public static SplitEnumerator SpanSplit(this Span<char> span, in char splitBy)
        {
            return new SplitEnumerator(str: span, new ReadOnlySpan<char>(in splitBy));
        }

        /// <summary>
        /// Splits the characters in the read-only span based on a single separator character.
        /// </summary>
        /// <param name="value">The read-only span of characters to split.</param>
        /// <param name="splitBy">The character to use as a separator.</param>
        /// <returns>A <see cref="SplitEnumerator"/> for enumerating over the split entries.</returns>
        [DebuggerStepThrough]
        public static SplitEnumerator SpanSplit(this ReadOnlySpan<char> value, in char splitBy)
        {
            return new SplitEnumerator(value, new ReadOnlySpan<char>(in splitBy));
        }
        /// <summary>
        /// Splits the characters in the read-only span based on a sequence of separator characters.
        /// </summary>
        /// <param name="value">The read-only span of characters to split.</param>
        /// <param name="splitBy">The span of characters to use as separators.</param>
        /// <returns>A <see cref="SplitEnumerator"/> for enumerating over the split entries.</returns>
        [DebuggerStepThrough]
        public static SplitEnumerator SpanSplit(this ReadOnlySpan<char> value, ReadOnlySpan<char> splitBy)
        {
            return new SplitEnumerator(value, splitBy);
        }
        /// <summary>
        /// Splits the string based on a single separator character.
        /// </summary>
        /// <param name="value">
        ///     The string to split. A <see langword="null"/> string is treated as an 
        ///     empty span.
        /// </param>
        /// <param name="splitBy">The character to use as a separator.</param>
        /// <returns>A <see cref="SplitEnumerator"/> for enumerating over the split entries.</returns>
        [DebuggerStepThrough]
        public static SplitEnumerator SpanSplit(this string? value, in char splitBy)
        {
            return new SplitEnumerator(value.AsSpan(), new ReadOnlySpan<char>(in splitBy));
        }
        /// <summary>
        /// Splits the string based on a sequence of separator characters.
        /// </summary>
        /// <param name="value">
        ///     The string to split. A <see langword="null"/> string is treated as an 
        ///     empty span.
        /// </param>
        /// <param name="splitBy">The span of characters to use as separators.</param>
        /// <returns>A <see cref="SplitEnumerator"/> for enumerating over the split entries.</returns>
        [DebuggerStepThrough]
        public static SplitEnumerator SpanSplit(this string? value, ReadOnlySpan<char> splitBy)
        {
            return new SplitEnumerator(value.AsSpan(), splitBy);
        }
    }
}

