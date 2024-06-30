using System.Buffers;

namespace AD.Api.Strings
{
    public interface IStringCreatable
    {
        int Length { get; }
        void WriteTo(scoped Span<char> chars);
    }

    file static class SpanStringCreate
    {
        internal static void WriteChars<T>(Span<char> chars, T state) where T : IStringCreatable
        {
            state.WriteTo(chars);
        }
    }
    public interface IStringCreatable<T> : IStringCreatable where T : IStringCreatable
    {
        static virtual string CreateString(ref readonly T value)
        {
            return string.Create(value.Length, value, SpanStringCreate.WriteChars);
        }
    }

    public static class StringCreatableExtensions
    {
        public static string CreateString<T>(this T value) where T : IStringCreatable<T>
        {
            return T.CreateString(ref value);
        }
    }
}

