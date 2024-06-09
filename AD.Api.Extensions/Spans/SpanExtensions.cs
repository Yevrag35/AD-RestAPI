using System;
using System.Buffers;
using System.Text;

namespace AD.Api.Spans
{
    /// <summary>
    /// Provides extension methods for <see cref="Span{T}"/>, <see cref="ReadOnlySpan{T}"/>, and 
    /// <see cref="string"/> instances when dealing with other spans.
    /// </summary>
    public static partial class SpanExtensions
    {
        /// <summary>
        /// Copies the characters of a string to a specified position within a target <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="value">The string whose characters are to be copied.</param>
        /// <param name="span">The target span where characters will be copied.</param>
        /// <param name="position">
        ///     The position within the target span to start copying characters to. This value is updated 
        ///     to reflect the new position after copying.
        /// </param>
        /// <inheritdoc cref="CopyToSlice(ReadOnlySpan{char}, Span{char}, ref int)" path="/exception"/>
        [DebuggerStepThrough]
        public static void CopyToSlice(this string? value, Span<char> span, scoped ref int position)
        {
            CopyToSlice(value.AsSpan(), span, ref position);
        }
        /// <summary>
        /// Copies the characters from a source <see cref="Span{T}"/> to a specified position within a 
        /// target <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="value">The source span whose characters are to be copied.</param>
        /// <param name="span">The target span where characters will be copied.</param>
        /// <param name="position">
        ///     The position within the target span to start copying characters to. This value is updated 
        ///     to reflect the new position after copying.
        /// </param>
        /// <inheritdoc cref="CopyToSlice{T}(ReadOnlySpan{T}, Span{T}, ref int)" path="/exception"/>
        [DebuggerStepThrough]
        public static void CopyToSlice<T>(this Span<T> value, Span<T> span, scoped ref int position)
        {
            CopyToSlice(readOnlySpan: value, span, ref position);
        }
        /// <summary>
        /// Copies the characters from a source <see cref="ReadOnlySpan{T}"/> to a specified position within
        /// a target <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="readOnlySpan">The source read-only span whose characters are to be copied.</param>
        /// <param name="span">The target span where characters will be copied.</param>
        /// <param name="position">
        ///     The position within the target span to start copying characters to. This value is updated 
        ///     to reflect the new position after copying.
        /// </param>
        /// <inheritdoc cref="ReadOnlySpan{T}.CopyTo(Span{T})" path="/exception"/>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if the specified position is less than 0 or if copying the characters would exceed 
        ///     the length of the target span.
        /// </exception>
        public static void CopyToSlice<T>(this ReadOnlySpan<T> readOnlySpan, Span<T> span, scoped ref int position)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(position);

            if (readOnlySpan.IsEmpty)
            {
                return;
            }
            else if (position == 0)
            {
                readOnlySpan.CopyTo(span);
            }
            else
            {
                readOnlySpan.CopyTo(span.Slice(position));
            }

            position += readOnlySpan.Length;
        }

        public static void CopyToSlice(this ReadOnlySpan<byte> byteSpan, Encoding encoding, Span<char> span, scoped ref int position)
        {
            position += encoding.GetChars(byteSpan, span.Slice(position));
        }
        /// <summary>
        /// Formats a value of a type implementing <see cref="ISpanFormattable"/> and copies it to a 
        /// specified position within a target <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the value, constrained to types implementing <see cref="ISpanFormattable"/>.
        /// </typeparam>
        /// <param name="value">The value to format and copy.</param>
        /// <param name="span">The target span where the formatted value will be copied.</param>
        /// <param name="position">
        ///     The position within the target span to start copying the formatted value to. This value is 
        ///     updated to reflect the new position after copying.
        /// </param>
        /// <param name="format">The format to use.</param>
        /// <param name="provider">The format provider to use.</param>
        /// <returns>The number of characters written to the target span.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if the specified position is less than 0.
        /// </exception>
        public static int CopyToSlice<T>(this T value, Span<char> span, ref int position, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
            where T : ISpanFormattable
        {
            ArgumentOutOfRangeException.ThrowIfNegative(position);

            int written;
            bool formatted = position == 0
                ? value.TryFormat(span, out written, format, provider)
                : value.TryFormat(span.Slice(position), out written, format, provider);

            if (formatted)
            {
                position += written;
            }

            return written;
        }

        [DebuggerStepThrough]
        public static Span<T> RentArray<T>(in int length, scoped ref bool isRented, scoped ref T[]? array)
        {
            Debug.Fail("Take a look at this ^");
            array = ArrayPool<T>.Shared.Rent(length);
            isRented = true;
            return array.AsSpan(0, length);
        }

        /// <summary>
        /// Tries to copy the characters of the string to the destination <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="value">The string whose characters are to be copied.</param>
        /// <param name="destination">The destination span where characters will be copied to.</param>
        /// <param name="charsWritten">The number of characters written to the destination span.</param>
        /// <returns>
        ///     <see langword="true"/> if the copy was successful; otherwise, <see langword="false"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool TryCopyToSlice([NotNullWhen(true)] this string? value, Span<char> destination, out int charsWritten)
        {
            if (value is null)
            {
                charsWritten = 0;
                return false;
            }

            return TryCopyToSlice(span: value, destination, out charsWritten);
        }
        /// <summary>
        /// Tries to copy the characters from a source <see cref="ReadOnlySpan{T}"/> to the destination 
        /// <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="span">The source read-only span whose characters are to be copied.</param>
        /// <param name="destination">The destination span where characters will be copied to.</param>
        /// <param name="charsWritten">The number of characters written to the destination span.</param>
        /// <returns>
        ///     <see langword="true"/> if the copy was successful or if this source span is empty; 
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryCopyToSlice<T>(this ReadOnlySpan<T> span, Span<T> destination, out int charsWritten)
        {
            if (span.IsEmpty || span.TryCopyTo(destination))
            {
                charsWritten = span.Length;
                return true;
            }
            else
            {
                charsWritten = 0;
                return false;
            }
        }
        /// <summary>
        /// Tries to copy the characters from a source <see cref="Span{T}"/> to the destination 
        /// <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="span">The source span whose characters are to be copied.</param>
        /// <param name="destination">The destination span where characters will be copied to.</param>
        /// <param name="charsWritten">The number of characters written to the destination span.</param>
        /// <returns>
        ///     <see langword="true"/> if the copy was successful or if this source span is empty; 
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryCopyToSlice<T>(this Span<T> span, Span<T> destination, out int charsWritten)
        {
            if (span.IsEmpty || span.TryCopyTo(destination))
            {
                charsWritten = span.Length;
                return true;
            }
            else
            {
                charsWritten = 0;
                return false;
            }
        }
    }
}

