namespace AD.Api.Buffers;

public static class Rent
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Array<T>(int minimumLength)
	{
		return ArrayPool<T>.Shared.Rent(minimumLength);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Return<T>(T[]? array, bool clearArray = false)
	{
		if (array is not null and { Length: > 0 })
		{
			ArrayPool<T>.Shared.Return(array, clearArray || RuntimeHelpers.IsReferenceOrContainsReferences<T>());
		}
	}
}