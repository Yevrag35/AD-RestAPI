using AD.Api.Collections;

namespace AD.Api.Extensions.Collections
{
    public static class ArrayExtensions
    {

        public static RentedArray<T> ToRentedArray<T>(this ReadOnlySpan<T> readOnlySpan)
        {
            return RentedArray<T>.FromSpan(readOnlySpan);
        }
        public static RentedArray<T> ToRentedArray<T>(this Span<T> span)
        {
            return RentedArray<T>.FromSpan(span);
        }
        public static RentedArray<T> ToRentedArray<T>(this T[] array, int length)
        {
            return ToRentedArray(array, 0, length);
        }
        public static RentedArray<T> ToRentedArray<T>(this T[] array, int index, int length)
        {
            ArgumentNullException.ThrowIfNull(array);

            return array.Length > 0
                ? ToRentedArray(span: array.AsSpan(index, length))
                : RentedArray.Empty<T>();
        }
    }
}

