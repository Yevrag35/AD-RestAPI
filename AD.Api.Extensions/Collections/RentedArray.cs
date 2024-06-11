using System.Buffers;

namespace AD.Api.Collections
{
    public static class RentedArray
    {
        public static RentedArray<T> Empty<T>() => default;
    }

    public struct RentedArray<T> : IDisposable
    {
        private T[] _array; int _length; bool _rented;

        public readonly T this[int index]
        {
            get => _array.AsSpan(0, _length)[index];
            set => _array[index] = value;
        }

        public readonly int Length => _length;
        public readonly bool IsRented => _rented;

        public RentedArray(T[] array, int length, bool isRented)
        {
            ArgumentNullException.ThrowIfNull(array);
            _length = length;
            _array = array;
            _rented = isRented;
        }
        public RentedArray(T[] array, int index, int length, bool isRented)
        {
            ArgumentNullException.ThrowIfNull(array);
            _length = length;

            if (index != 0)
            {
                T[] rented = ArrayPool<T>.Shared.Rent(length);
                array.AsSpan(index, length).CopyTo(rented);
                if (isRented)
                {
                   ArrayPool<T>.Shared.Return(array);
                }

                array = rented;
                isRented = true;
            }

            _array = array;
            _rented = isRented;
        }
        private RentedArray(ReadOnlySpan<T> values)
        {
            _length = values.Length;
            T[] array = ArrayPool<T>.Shared.Rent(values.Length);
            _rented = true;

            values.CopyTo(array);
            _array = array;
        }

        public readonly Memory<T> AsMemory()
        {
            return _array.AsMemory(0, _length);
        }
        public readonly Span<T> AsSpan()
        {
            return _array.AsSpan(0, _length);
        }
        /// <summary>
        /// Returns the backing array to the pool if it was rented.
        /// </summary>
        public void Dispose()
        {
            bool isRented = _rented;
            T[]? array = _array;
            this = default;

            if (isRented && array is not null)
            {
                ArrayPool<T>.Shared.Return(array);
            }
        }

        internal static RentedArray<T> FromSpan(ReadOnlySpan<T> span)
        {
            return new(span);
        }

        public static implicit operator RentedArray<T>(T[] array)
        {
            return new(array, array.Length, isRented: false);
        }
        public static implicit operator Span<T>(RentedArray<T> rented)
        {
            return rented.AsSpan();
        }
    }
}

