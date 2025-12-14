namespace AD.Api.Collections.Extensions;

public static class ArrayExtensions
{
	/// <summary>
	/// Converts an <see cref="IEnumerable{T}"/> to an array.
	/// </summary>
	/// <remarks>
	/// If the input sequence is already an array, it is returned with no allocation. 
	/// Otherwise, creates a new array from the specified <see cref="IEnumerable{T}"/>.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	/// <param name="source"></param>
	/// <returns>
	/// The input sequence casted to an array or allocates a new array that contains the copied elements from 
	/// the input sequence.
	/// </returns>
	/// <inheritdoc cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})" path="/exception"/>
	public static T[] AsOrToArray<T>(this IEnumerable<T> source)
	{
		return source is T[] array
			? array
			: [.. source];
	}
	public static RentedArray<T> ToRentedArray<T>(this ReadOnlySpan<T> readOnlySpan)
	{
		return new(readOnlySpan);
	}
	public static RentedArray<T> ToRentedArray<T>(this T[] array, int length)
	{
		return ToRentedArray(array, 0, length);
	}
	public static RentedArray<T> ToRentedArray<T>(this T[] array, int index, int length)
	{
		ArgumentNullException.ThrowIfNull(array);

		return array.Length > 0
			? ToRentedArray(array.AsSpan(index, length))
			: [];
	}
}

