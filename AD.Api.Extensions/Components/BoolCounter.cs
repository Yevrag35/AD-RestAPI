namespace AD.Api.Components
{
    /// <summary>
    /// A ref struct that keeps track of the number of <see langword="true"/> boolean values that have been set.
    /// </summary>
    public ref struct BoolCounter
    {
        private int _count;
        private readonly Span<bool> _counted;

        /// <summary>
        /// The number of <see langword="true"/> boolean values that have been set.
        /// </summary>
        public readonly int Count => _count;

        public BoolCounter(Span<bool> counted)
        {
            _counted = counted;
            _count = 0;
        }

        private readonly bool IndexHasFlag(in int flag)
        {
            return _counted[flag];
        }

        public bool MarkFlag(int flag, bool value)
        {
            return value && this.MarkFlag(in flag);
        }
        public bool MarkFlag(in int flag)
        {
            bool result = false;

            if (!this.IndexHasFlag(in flag))
            {
                result = true;
                _counted[flag] = result;
                _count++;
            }

            return result;
        }

        public readonly bool MoveNext()
        {
            return _count < _counted.Length;
        }
    }
}

