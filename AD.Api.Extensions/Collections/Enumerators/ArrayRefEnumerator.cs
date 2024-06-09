using System.Runtime.InteropServices;

namespace AD.Api.Collections.Enumerators
{
    /// <summary>
    /// An efficient ref struct enumerator for <see cref="ReadOnlySpan{T}"/> and <see cref="List{T}"/> types that 
    /// allows for enumeration and breaking on a conditionally passed flag.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerStepThrough]
    public ref struct ArrayRefEnumerator<T>
    {
        private readonly ReadOnlySpan<T> _array;
        private int _index;

        /// <summary>
        /// The current element in the collection.
        /// </summary>
        public readonly T Current => _array[_index];

        public ArrayRefEnumerator(List<T> list)
            : this(CollectionsMarshal.AsSpan(list))
        {
        }
        public ArrayRefEnumerator(ReadOnlySpan<T> array)
        {
            _array = array;
            _index = -1;
        }

        public bool MoveNext()
        {
            bool flag = false;
            return this.MoveNext(in flag);
        }
        public bool MoveNext(in bool flag)
        {
            if (flag)
            {
                _index = _array.Length;
                return false;
            }

            int next = _index + 1;
            int length = _array.Length;
            if ((uint)next >= (uint)length)
            {
                _index = length;
                return false;
            }

            _index = next;
            return true;
        }
        public bool MoveNext(in bool flag, ref int index)
        {
            if (flag)
            {
                index = _index;
                _index = _array.Length;
                return false;
            }

            int next = _index + 1;
            int length = _array.Length;
            if ((uint)next >= (uint)length)
            {
                _index = length;
                return false;
            }

            _index = next;
            return true;
        }
        public void Reset()
        {
            _index = -1;
        }
    }
}

