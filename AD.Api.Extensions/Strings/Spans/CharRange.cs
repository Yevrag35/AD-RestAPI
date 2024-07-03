namespace AD.Api.Strings.Spans
{
    public ref struct CharRange
    {
        private const int MIN_CHAR = char.MinValue;
        private const int MAX_CHAR = char.MaxValue;

        private char _start;
        private char _end;
        private int _length;

        public readonly int Length => _length;
        public char Start
        {
            readonly get => _start;
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, _end, nameof(this.Start));
                if (value == _start)
                {
                    return;
                }

                _start = value;
                _length = GetLength(ref value, ref _end);
            }
        }
        public char End
        {
            readonly get => _end;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, _start, nameof(this.End));
                if (value == _end)
                {
                    return;
                }

                _end = value;
                _length = GetLength(ref _start, ref value);
            }
        }

        /// <inheritdoc cref="ValidateRange(int, int)" path="/exception"/>
        public CharRange(char start, char end)
        {
            ValidateRange(ref start, ref end);
            _start = start;
            _end = end;
            _length = GetLength(ref start, ref end);
        }

        public readonly void CopyTo(scoped Span<char> span)
        {
            int length = this.Length;
            ArgumentOutOfRangeException.ThrowIfLessThan(span.Length, length, nameof(span));

            char start = this.Start;

            for (int i = 0; i < length; i++)
            {
                span[i] = (char)(start + i);
            }
        }
        private static int GetLength(ref readonly char start, ref readonly char end)
        {
            return end - start + 1;
        }
        public readonly char[] ToArray()
        {
            char[] array = new char[this.Length];
            this.CopyTo(array);
            return array;
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        private static void ValidateRange(ref readonly char start, ref readonly char end)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start, end);
        }

        public static implicit operator CharRange(Span<char> span)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(span.Length, 2, nameof(span));
            char start = span[0];
            char end = span[^1];

            return end < start ? new CharRange(end, start) : new CharRange(start, end);
        }
    }
}

