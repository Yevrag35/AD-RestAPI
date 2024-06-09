namespace AD.Api.Strings.Spans
{
    /// <summary>
    /// Represents an entry in a split sequence, containing both the characters before the separator and 
    /// the separator itself.
    /// </summary>
    [DebuggerStepThrough]
    public readonly ref struct SplitEntry
    {
        /// <summary>
        /// Gets the slice of characters before the separator.
        /// </summary>
        public readonly ReadOnlySpan<char> Chars;
        /// <summary>
        /// Gets the separator characters used.
        /// </summary>
        public readonly ReadOnlySpan<char> Separator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitEntry"/> struct.
        /// </summary>
        /// <param name="chars">The characters before the separator.</param>
        /// <param name="separator">The separator characters.</param>
        public SplitEntry(ReadOnlySpan<char> chars, ReadOnlySpan<char> separator)
        {
            Chars = chars;
            Separator = separator;
        }

        /// <summary>
        /// Deconstructs the <see cref="SplitEntry"/> into its component characters and separator.
        /// </summary>
        /// <param name="chars">When this method returns, contains the characters before the separator.</param>
        /// <param name="separator">When this method returns, contains the separator characters.</param>
        public readonly void Deconstruct(out ReadOnlySpan<char> chars, out ReadOnlySpan<char> separator)
        {
            chars = Chars;
            separator = Separator;
        }

        /// <summary>
        /// Implicitly converts a <see cref="SplitEntry"/> to a <see cref="ReadOnlySpan{T}"/>, returning 
        /// its characters before the separator.
        /// </summary>
        /// <param name="entry">The split entry to convert.</param>
        public static implicit operator ReadOnlySpan<char>(SplitEntry entry) => entry.Chars;
    }
}

