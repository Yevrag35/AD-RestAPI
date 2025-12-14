namespace AD.Api.Buffers;

public static class ArrayHelper
{
	/// <summary>
	/// Returns the specified array to the shared array pool.
	/// </summary>
	/// <typeparam name="T">The type of elements in the array.</typeparam>
	/// <param name="array">The array to return to the pool. Can be <see langword="null"/>.</param>
	/// <param name="forceClear">A value indicating whether the array should be cleared before being returned to the pool.  If <see
	/// langword="true"/>, the array is cleared; otherwise, it is cleared only if it contains reference types or
	/// references.</param>
	/// <returns><see langword="true"/> if the array was successfully returned to the pool; otherwise, <see langword="false"/>.</returns>
	public static bool ReturnToPool<T>(T[]? array, bool forceClear = false)
	{
		bool returned = false;
		if (array is not null and { Length: > 0 })
		{
			ArrayPool<T>.Shared.Return(array, forceClear || RuntimeHelpers.IsReferenceOrContainsReferences<T>());
			returned = true;
		}

		return returned;
	}
}
