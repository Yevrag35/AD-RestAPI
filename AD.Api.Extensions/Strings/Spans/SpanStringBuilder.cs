using AD.Api.Spans;
using System.Buffers;
using System.Numerics;
using System.Text;

namespace AD.Api.Strings.Spans
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <param name="state"></param>
    /// <returns>
    /// The number of characters written to <paramref name="span"/>.
    /// </returns>
    public delegate int WriteToSpan<T>(Span<char> span, T state);

    /// <summary>
    /// A ref struct that provides a way to build strings using a <see cref="Span{char}"/> buffer.
    /// </summary>
    public ref struct SpanStringBuilder
    {
        const int MULTIPLE = 128;
        const int INCREMENT = MULTIPLE - 1;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        static readonly string NEW_LINE = Environment.NewLine;
        static readonly int NEW_LINE_LENGTH = NEW_LINE.Length;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _isRented;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _position;

        private char[]? _array;
        private Span<char> _span;

        public ref char this[int index] => ref _span[index];

        public readonly int Capacity => _array?.Length ?? _span.Length;
        [MemberNotNullWhen(true, nameof(_array))]
        public readonly bool IsRented => _isRented;
        public readonly int Length => _position;

        public SpanStringBuilder(int minimumCapacity)
        {
            int capacity = AdjustCapacity(in minimumCapacity);
            char[] array = ArrayPool<char>.Shared.Rent(capacity);
            _array = array;
            _isRented = true;
            _span = array;
            _position = 0;
        }
        [DebuggerStepThrough]
        public SpanStringBuilder(Span<char> initialBuffer)
        {
            _array = null;
            _isRented = false;
            _span = initialBuffer;
            _position = 0;
        }

        /// <summary>
        /// Allocates a new <see cref="string"/> from the characters appended to the builder and disposes this
        /// <see cref="SpanStringBuilder"/>.
        /// </summary>
        /// <remarks>
        ///     Use <see cref="ToString"/> if you want to construct additional strings from the same builder.
        /// </remarks>
        /// <returns>
        ///     The constructed <see cref="string"/> instance.
        /// </returns>
        public string Build()
        {
            string str = this.ToString();
            this.Dispose();
            return str;
        }

        /// <inheritdoc cref="StringBuilder.Append(char)"/>
        public SpanStringBuilder Append(char value)
        {
            this.EnsureCapacity(1);
            _span[_position++] = value;
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified <typeparamref name="T"/> value to this instance.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="INumber{TSelf}"/> being appended.</typeparam>
        /// <param name="number">The number to append.</param>
        /// <param name="format">The format to use.</param>
        /// <param name="provider">The format provider to use.</param>
        /// <returns>
        /// <inheritdoc cref="Append(char)"/>
        /// </returns>
        public SpanStringBuilder Append<T>(T number, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : INumber<T>, IMinMaxValue<T>
        {
            int length = number.GetLength();
            this.EnsureCapacity(length);
            number.CopyToSlice(_span, ref _position, format, provider);

            return this;
        }

        /// <inheritdoc cref="StringBuilder.Append(char, int)"/>
        public SpanStringBuilder Append(char value, int count)
        {
            this.EnsureCapacity(count);

            int pos = _position;
            _span.Slice(pos, count).Fill(value);
            _position += count;

            return this;
        }

        /// <inheritdoc cref="StringBuilder.Append(ReadOnlySpan{char})"/>
        /// <inheritdoc cref="EnsureCapacity(int)" path="/exception"/>
        public SpanStringBuilder Append(scoped ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                return this;
            }

            int pos = _position;
            this.EnsureCapacity(value.Length);
            value.CopyToSlice(_span, ref pos);

            _position = pos;
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified <typeparamref name="T"/> value to this instance.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ISpanFormattable"/> being appended.</typeparam>
        /// <param name="formattable">The formattable value to append.</param>
        /// <param name="format">The format to use.</param>
        /// <param name="provider">The format provider to use.</param>
        /// <returns>
        /// <inheritdoc cref="Append(char)"/>
        /// </returns>
        /// <inheritdoc cref="EnsureCapacity(int)" path="/exception"/>
        public SpanStringBuilder Append<T>(T formattable, int maxLength, scoped ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            this.EnsureCapacity(maxLength);
            formattable.CopyToSlice(_span, ref _position, format, provider);

            return this;
        }

        public SpanStringBuilder Append<T>(int length, T state, SpanAction<char, T> spanAction)
        {
            this.EnsureCapacity(length);
            spanAction(_span.Slice(_position, length), state);
            _position += length;

            return this;
        }
        public SpanStringBuilder Append<T>(int maxLength, T state, WriteToSpan<T> spanFunc)
        {
            this.EnsureCapacity(maxLength);
            int written = spanFunc(_span.Slice(_position, maxLength), state);
            _position += written;

            return this;
        }

        /// <inheritdoc cref="StringBuilder.AppendLine"/>
        public SpanStringBuilder AppendLine()
        {
            this.EnsureCapacity(NEW_LINE_LENGTH);
            NEW_LINE.CopyToSlice(_span, ref _position);
            return this;
        }
        /// <inheritdoc cref="StringBuilder.AppendLine(string)"/>
        public SpanStringBuilder AppendLine(scoped ReadOnlySpan<char> value)
        {
            this.EnsureCapacity(value.Length + NEW_LINE_LENGTH);
            value.CopyToSlice(_span, ref _position);
            NEW_LINE.CopyToSlice(_span, ref _position);

            return this;
        }
        public readonly Span<char> AsSpan()
        {
            return _span.Slice(0, _position);
        }
        public readonly Span<char> AsUnwrittenSpan()
        {
            return _span.Slice(_position);
        }

        public readonly ReadOnlySpan<char> GetSegment(int start, int length)
        {
            return _span.Slice(start, length);
        }

        [DebuggerStepThrough]
        public readonly int IndexOf(char value)
        {
            return _span.IndexOf(value);
        }
        [DebuggerStepThrough]
        public readonly int IndexOf(char value, int startIndex)
        {
            return _span.Slice(startIndex).IndexOf(value);
        }
        [DebuggerStepThrough]
        public readonly int IndexOf(char value, int startIndex, int count)
        {
            return _span.Slice(startIndex, count).IndexOf(value);
        }

        [DebuggerStepThrough]
        public SpanStringBuilder Insert(int index, char c)
        {
            return this.Insert(index, c, 1);
        }
        public SpanStringBuilder Insert(int index, char c, int count)
        {
            this.EnsureCapacity(count);
            int remaining = _position - index;

            _span.Slice(index, remaining).CopyTo(_span.Slice(index + count));
            _span.Slice(index, count).Fill(c);

            _position += count;
            return this;
        }
        public SpanStringBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                return this;
            }

            this.EnsureCapacity(value.Length);
            int remaining = _position - index;
            _span.Slice(index, remaining).CopyTo(_span.Slice(index + value.Length));
            value.CopyTo(_span.Slice(index));

            _position += value.Length;
            return this;
        }

        public SpanStringBuilder Remove(int startIndex, int length)
        {
            int position = _position;
            int newLength = startIndex + length;
            // Validate parameters
            ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startIndex, position);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(newLength, position, paramName: "'startIndex' and 'length'");

            // Calculate the number of elements to move
            int moveCount = position - newLength;

            // Shift elements to the left; CopyTo handles overlapping memory regions correctly
            if (moveCount > 0)
            {
                _span.Slice(newLength, moveCount).CopyTo(_span.Slice(startIndex));
            }

            // Update position
            _position -= length;
            return this;
        }

        /// <summary>
        /// Allocates a new <see cref="string"/> from the characters appended to the builder.
        /// </summary>
        /// <returns>
        /// The constructed <see cref="string"/> instance.
        /// </returns>
        [DebuggerStepThrough]
        public override readonly string ToString()
        {
            string s = _span.Slice(0, _position).ToString();
            return s;
        }

        private static int AdjustCapacity(in int capacity)
        {
            return (capacity + INCREMENT) & ~INCREMENT;
        }
        /// <exception cref="ArgumentOutOfRangeException"/>
        private void EnsureCapacity(int appendLength)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(appendLength);
            int calculatedLength = _position + appendLength;
            if (calculatedLength > this.Capacity)
            {
                this.Grow(calculatedLength);
            }
        }
        private void Grow(int minimumCapacity)
        {
            Debug.Assert(minimumCapacity >= this.Capacity);
            int newCapacity = AdjustCapacity(in minimumCapacity);
            Debug.Assert(newCapacity % MULTIPLE == 0);

            char[] newArray = ArrayPool<char>.Shared.Rent(newCapacity);
            _span.Slice(0, _position).CopyTo(newArray);
            if (this.IsRented)
            {
                ArrayPool<char>.Shared.Return(_array);
            }
            else
            {
                _isRented = true;
            }

            _array = newArray;
            _span = newArray;
        }

        public void Dispose()
        {
            char[]? array = _array;
            bool isRented = _isRented;
            this = default;

            if (isRented)
            {
                ArrayPool<char>.Shared.Return(array!);
            }
        }
    }
}

