namespace AD.Api.Spans
{
    /// <summary>
    /// A read-only struct that represents a position and length range within a given span.
    /// </summary>
    [DebuggerStepThrough]
    [DebuggerDisplay(@"\{Index={Index}, Length={Length}\}")]
    public readonly struct SpanPosition : IComparable<int>, IComparable<SpanPosition>, IEquatable<int>, IEquatable<SpanPosition>
    {
        /// <summary>
        /// The starting index of the segment within a given span.
        /// </summary>
        public readonly int Index;
        /// <summary>
        /// The length of the segment within a given span.
        /// </summary>
        public readonly int Length;

        public SpanPosition(int start, int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            Index = start;
            Length = length;
        }

        public readonly int CompareTo(int other)
        {
            return Index.CompareTo(other);
        }
        public readonly int CompareTo(SpanPosition other)
        {
            return this.CompareTo(other.Index);
        }
        public readonly bool Equals(int other)
        {
            return Index == other;
        }
        public readonly bool Equals(SpanPosition other)
        {
            return Index == other.Index;
        }
        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return (obj is SpanPosition other && this.Equals(other))
                   ||
                   (obj is int i && this.Equals(i));
        }
        public override readonly int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static implicit operator SpanPosition(int start)
        {
            return new(start, 0);
        }

        public static bool operator ==(SpanPosition left, SpanPosition right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(SpanPosition left, SpanPosition right)
        {
            return !left.Equals(right);
        }
    }
}

