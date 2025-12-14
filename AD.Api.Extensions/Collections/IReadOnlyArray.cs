namespace AD.Api.Collections;

/// <summary>
/// Represents a read-only array of elements that provides indexed access and supports conversion to a <see
/// cref="ReadOnlySpan{T}"/>.
/// </summary>
/// <remarks>This interface extends <see cref="IReadOnlyList{T}"/> to include
/// functionality for obtaining a <see cref="ReadOnlySpan{T}"/> representation of the array.</remarks>
/// <typeparam name="T">The type of elements in the array.</typeparam>
public interface IReadOnlyArray<T> : IReadOnlyList<T>
{
	/// <summary>
	/// Returns a read-only span that represents the elements of the current collection.
	/// </summary>
	/// <remarks>The returned <see cref="ReadOnlySpan{T}"/> provides a memory-efficient view of the collection's
	/// data  without requiring additional allocations.</remarks>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> that represents the elements of the collection.</returns>
	ReadOnlySpan<T> AsSpan();
}