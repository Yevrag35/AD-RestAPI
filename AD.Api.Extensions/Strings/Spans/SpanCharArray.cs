using AD.Api.Spans;
using AD.Api.Strings.Extensions;
using System.Buffers;

namespace AD.Api.Strings.Spans
{
    /// <summary>
    /// A ref struct that provides a way to build a string from multiple segments with a combined separator.
    /// </summary>
    /// <remarks>
    /// This struct should be disposed after use to release the rented memory.
    /// </remarks>
    public ref struct SpanCharArray
    {
        const int MULTIPLE = 16;
        const int INCREMENT = MULTIPLE - 1;

        private SpanPosition[]? _positionArray;
        private Span<SpanPosition> _positions;
        private SpanStringBuilder _builder;
        private int _index;
        private bool _isRented;
        private char _separator;

        /// <summary>
        /// Gets the read-only span at the specified index.
        /// </summary>
        /// <param name="index">The index within the array to use.</param>
        /// <returns>
        /// The <see cref="ReadOnlySpan{T}"/> at the specified index.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public readonly ReadOnlySpan<char> this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfNegative(index);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _positions.Length);
                return this.GetSegment(index);
            }
        }
        public readonly int Capacity => _positionArray?.Length ?? _positions.Length;
        public readonly int Count => _index;
        [MemberNotNullWhen(true, nameof(_positionArray))]
        public readonly bool IsRented => _isRented;

        public SpanCharArray(int minimumLength, char separator)
        {
            SpanPosition[] positionArray = ArrayPool<SpanPosition>.Shared.Rent(MULTIPLE);
            _isRented = true;
            _positionArray = positionArray;
            _positions = positionArray;
            _separator = separator;
            _index = 0;
            minimumLength = Math.Max(1, minimumLength);
            _builder = new(minimumLength);
        }

        [DebuggerStepThrough]
        public SpanCharArray Add(scoped ReadOnlySpan<char> value)
        {
            this.EnsureCapacity(1);

            this.AddSegment(value);
            return this;
        }
        public SpanCharArray AddRange(scoped ReadOnlySpan<char> value, scoped ReadOnlySpan<char> splitBy)
        {
            int count = value.Count(splitBy) + 1;
            this.EnsureCapacity(count);

            foreach (ReadOnlySpan<char> section in value.SpanSplit(splitBy))
            {
                this.AddSegment(section);
            }

            return this;
        }
        private void AddSegment(scoped ReadOnlySpan<char> value)
        {
            bool noSpace = _index == 0;
            int index = _builder.Length;
            if (!noSpace)
            {
                index++;
            }

            SpanPosition pos = new(index, value.Length);
            _positions[_index++] = pos;
            if (noSpace)
            {
                _builder = _builder.Append(value);
                return;
            }

            int length = value.Length + 1;
            Span<char> buffer = stackalloc char[length];
            buffer[0] = _separator;
            value.CopyTo(buffer.Slice(1));
            _builder = _builder.Append(buffer);
        }

        public readonly ReadOnlySpan<char> AsSpan()
        {
            return _builder.AsSpan();
        }

        [DebuggerStepThrough]
        public readonly bool Contains(scoped ReadOnlySpan<char> value)
        {
            return this.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
        public readonly bool Contains(scoped ReadOnlySpan<char> value, StringComparison comparison)
        {
            int index = _index;
            for (int i = 0; i < index; i++)
            {
                if (this.GetSegment(i).Equals(value, comparison))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Allocates a new <see cref="string"/> from the spans added and disposes this
        /// <see cref="SpanStringBuilder"/>.
        /// </summary>
        /// <remarks>
        ///     Use <see cref="ToString"/> if you want to construct additional strings from the same array.
        /// </remarks>
        /// <returns>
        ///     The constructed <see cref="string"/> instance.
        /// </returns>
        [DebuggerStepThrough]
        public string Build()
        {
            string result = this.ToString();
            this.Dispose();
            return result;
        }
        public void Dispose()
        {
            bool isRented = this.IsRented;
            SpanPosition[]? array = _positionArray;
            _builder.Dispose();
            this = default;

            if (isRented)
            {
                ArrayPool<SpanPosition>.Shared.Return(array!);
            }
        }

        [DebuggerStepThrough]
        [Obsolete("A bug exists in the 'Remove' methods.", error: true)]
        public bool Remove(scoped ReadOnlySpan<char> value)
        {
            return this.Remove(value, StringComparison.OrdinalIgnoreCase);
        }
        [Obsolete("A bug exists in the 'Remove' methods.", error: true)]
        public bool Remove(scoped ReadOnlySpan<char> value, StringComparison comparison)
        {
            bool removed = false;
            int index = _index;
            for (int i = 0; i < index; i++)
            {
                ref readonly SpanPosition pos = ref _positions[i];
                if (_builder.GetSegment(pos.Index, pos.Length).Equals(value, comparison))
                {
                    this.RemoveAt(in i, in pos);

                    removed = true;
                    break;
                }
            }

            return removed;
        }
        [Obsolete("A bug exists in the 'Remove' methods.", error: true)]
        public void RemoveAt(int index)
        {
            ref readonly SpanPosition pos = ref _positions[index];
            this.RemoveAt(in index, in pos);
        }
        [Obsolete("A bug exists in the 'Remove' methods.", error: true)]
        private void RemoveAt(in int index, ref readonly SpanPosition pos)
        {
            if (pos.Index + pos.Length < _builder.Length)
            {
                _builder = _builder.Remove(pos.Index, pos.Length + 1); // 1 for the space
            }
            else
            {
                _builder = _builder.Remove(pos.Index, pos.Length);
            }

            Span<SpanPosition> allPos = _positions;
            Span<SpanPosition> posAtIndex = allPos.Slice(index);
            int minusOne = posAtIndex.Length - 1;

            for (int i = 0; i < posAtIndex.Length; i++)
            {
                posAtIndex[i] = i < minusOne
                    ? posAtIndex[i + 1].ShiftLeft(pos.Length)
                    : default;
            }

            _positions = allPos.Slice(0, allPos.Length - 1);
        }
        /// <summary>
        /// Allocates a new <see cref="string"/> from the character spans added to this <see cref="SpanCharArray"/>.
        /// </summary>
        /// <returns>
        /// The constructed <see cref="string"/> instance.
        /// </returns>
        [DebuggerStepThrough]
        public override readonly string ToString()
        {
            return _builder.ToString();
        }

        public static SpanCharArray Split(scoped ReadOnlySpan<char> value, char separator)
        {
            if (value.IsWhiteSpace())
            {
                return new(16, separator);
            }

            int count = value.Count(separator) + 1;
            SpanCharArray array = new(count, separator);

            foreach (ReadOnlySpan<char> section in value.SpanSplit(in separator))
            {
                array.AddSegment(section);
            }

            return array;
        }

        [DebuggerStepThrough]
        private static int AdjustCapacity(in int capacity)
        {
            return (capacity + INCREMENT) & ~INCREMENT;
        }
        /// <exception cref="ArgumentOutOfRangeException"/>
        [DebuggerStepThrough]
        private void EnsureCapacity(int appendLength)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(appendLength);
            int calculatedLength = _index + appendLength;
            if (calculatedLength > this.Capacity)
            {
                this.Grow(calculatedLength);
            }
        }
        [DebuggerStepThrough]
        private readonly ReadOnlySpan<char> GetSegment(int index)
        {
            ref readonly SpanPosition pos = ref _positions[index];
            return _builder.GetSegment(pos.Index, pos.Length);
        }
        [DebuggerStepThrough]
        private void Grow(int minimumCapacity)
        {
            Debug.Assert(minimumCapacity >= this.Capacity);
            int newCapacity = AdjustCapacity(in minimumCapacity);
            Debug.Assert(newCapacity % MULTIPLE == 0);

            SpanPosition[] newArray = ArrayPool<SpanPosition>.Shared.Rent(newCapacity);
            _positions.Slice(0, _index).CopyTo(newArray);
            if (this.IsRented)
            {
                ArrayPool<SpanPosition>.Shared.Return(_positionArray);
            }
            else
            {
                _isRented = true;
            }

            _positionArray = newArray;
            _positions = newArray;
        }
    }
}

