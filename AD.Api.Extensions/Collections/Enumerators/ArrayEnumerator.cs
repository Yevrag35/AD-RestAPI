using AD.Api.Extensions.Collections;
using System.Collections;
using System.Runtime.InteropServices;

namespace AD.Api.Collections.Enumerators
{
    [DebuggerStepThrough]
    [StructLayout(LayoutKind.Auto)]
    public struct ArrayEnumerator<T> : IEnumerator<T>
    {
        private RentedArray<T> _array; int _index; bool _notDisposed;

        public readonly T Current => _array[_index];
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly object? IEnumerator.Current => this.Current;

        public ArrayEnumerator(ReadOnlySpan<T> span)
        {
            _array = span.ToRentedArray();
            _index = -1;
            _notDisposed = true;
        }

        public bool MoveNext()
        {
            ObjectDisposedException.ThrowIf(!_notDisposed, typeof(ArrayEnumerator<T>));
            int index = _index + 1;
            int length = _array.Length;

            if ((uint)index >= (uint)length)
            {
                _index = length;
                return false;
            }

            _index = index;
            return true;
        }
        public void Reset()
        {
            ObjectDisposedException.ThrowIf(!_notDisposed, typeof(ArrayEnumerator<T>));
            _index = -1;
        }

        #region DISPOSING
        public void Dispose()
        {
            RentedArray<T> array = _array;
            this = default;

            array.Dispose();
        }

        #endregion
    }
}

