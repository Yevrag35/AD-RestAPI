using AD.Api.Strings.Spans;

namespace AD.Api.Strings.Extensions
{
    public static partial class StringExtensions
    {
        public static SplitAnyEnumerator SpanSplitAny(this ReadOnlySpan<char> chars, ReadOnlySpan<char> splitByAny)
        {
            return new SplitAnyEnumerator(chars, splitByAny);
        }
        public static SplitAnyEnumerator SpanSplitAny(this string? value, ReadOnlySpan<char> splitByAny)
        {
            return new SplitAnyEnumerator(value.AsSpan(), splitByAny);
        }
    }
}

