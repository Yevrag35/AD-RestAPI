namespace AD.Api.Collections.Intrinsics;

internal static class ArrayListMarshal
{
	//private const int ARRAYLIST_FIELD_COUNT = 3;

	//static ArrayListMarshal()
	//{
	//	if (ARRAYLIST_FIELD_COUNT != typeof(ArrayList).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Length)
	//	{
	//		throw new Exception("ArrayList has changed its internal structure.  This code may not work as expected.");
	//	}
	//}

	internal static ReadOnlyMemory<T> AsMemory<T>(ArrayList list)
	{
		Debug.Assert(!list.IsSynchronized, "This method does not support synchronized ArrayLists.");
		int length = list.Count;
		if (length == 0)
			return ReadOnlyMemory<T>.Empty;

		T[] array = AsArray<T>(list);

		return new(array, 0, length);
	}
	/// <summary>
	/// Retrieves the underlying object array and reinterprets it as a span of <typeparamref name="T"/> elements.
	/// </summary>
	/// <param name="list">The <see cref="LdapPropertyList"/> containing the strings to include in the span. Must not be null.</param>
	/// <returns>A <see cref="Span{T}"/> of <typeparamref name="T"/> representing the elements in the <paramref name="list"/>.  Returns an empty span
	/// if the list is empty.</returns>
	internal static Span<T> AsSpan<T>(ArrayList list)
	{
		Debug.Assert(!list.IsSynchronized, "This method does not support synchronized ArrayLists.");

		int length = list.Count;
		if (length == 0)
			return [];

		ref T first = ref GetFirstItemUnsafe<T>(list);

		return MemoryMarshal.CreateSpan(ref first, length);
	}
	/// <summary>
	/// Retrieves a reference to the object of type <typeparamref name="T"/> at the specified index in the given list.
	/// </summary>
	/// <param name="list">The <see cref="ArrayList"/> containing the objects.</param>
	/// <param name="index">The zero-based index of the object to retrieve. Must be within the range of the list.</param>
	/// <returns>A read-only reference to the object at the specified <paramref name="index"/> in the <paramref name="list"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than or equal to zero or greater than or equal to the number of elements in the <paramref name="list"/>.</exception>
	internal static ref T ItemRef<T>(ArrayList list, int index)
	{
		Debug.Assert(!list.IsSynchronized, "This method does not support synchronized ArrayLists.");
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)list.Count, nameof(index));
		ref T first = ref GetFirstItemUnsafe<T>(list);
		return ref Unsafe.Add(ref first, index);
	}

	internal static T[] AsArray<T>(ArrayList list)
	{
		Debug.Assert(!list.IsSynchronized, "This method does not support synchronized ArrayLists.");
		//var view = Unsafe.As<ArrayListView>(list);
		return Unsafe.As<T[]>(GetItems(list));
	}
	internal static T[] AsArray<T>(ArrayList list, out int length)
	{
		Debug.Assert(!list.IsSynchronized, "This method does not support synchronized ArrayLists.");
		length = list.Count;
		object[] items = GetItems(list);
		return Unsafe.As<T[]>(items);
	}
	internal static object[] AsRawArray(ArrayList list)
	{
		return GetItems(list) ?? [];
	}
	private static ref T GetFirstItemUnsafe<T>(ArrayList list)
	{
		Debug.Assert(list.Count > 0, "List must contain at least one item.");
		return ref Unsafe.As<object, T>(ref MemoryMarshal.GetArrayDataReference(GetItems(list)));
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
	private static extern ref object[] GetItems(ArrayList list);
}