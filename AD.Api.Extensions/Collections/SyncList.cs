using AD.Api.Buffers;

namespace AD.Api.Collections;

/// <summary>
/// Provides static methods for creating and working with synchronized lists.
/// </summary>
public static class SyncList
{
	/// <summary>
	/// Creates a new <see cref="SyncList{T}"/> containing the specified values.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	/// <param name="values">A read-only span of values to include in the new list.</param>
	/// <returns>A new <see cref="SyncList{T}"/> that contains the specified values.</returns>
	public static SyncList<T> Create<T>(params ReadOnlySpan<T> values)
	{
		return new(values);
	}
}

/// <summary>
/// Represents a list that can be configured for thread-safe or non-thread-safe access, providing synchronized
/// operations for managing a collection of elements.
/// </summary>
/// <remarks><see cref="SyncList{T}"/> enables safe concurrent access to its elements when in thread-safe mode, using internal
/// locking to synchronize operations. Once marked as not thread-safe via <see cref="MarkNotThreadSafe"/>, the instance disables
/// locking permanently and should only be accessed from a single thread. The thread safety mode is determined at
/// construction and can be changed only once. This class implements <see cref="IList{T}"/> and provides standard list operations,
/// with additional support for derived classes to customize core behaviors by overriding protected virtual
/// methods.</remarks>
/// <typeparam name="T">The type of elements contained in the list.</typeparam>
[CollectionBuilder(typeof(SyncList), nameof(SyncList.Create))]
[DebuggerDisplay(@"Count = {Count} \{IsThreadSafe = {IsThreadSafe}\}")]
public partial class SyncList<T> : IList<T>, IReadOnlyList<T>
{
	private readonly Lock _lock = new();
	/// <summary>
	/// 0 = ThreadSafe, 1 = NotThreadSafe (permanent)
	/// </summary>
	private int _mode;
	/// <summary>
	/// The underlying list storing the elements. Accessing this field directly is not thread-safe.
	/// </summary>
	private readonly List<T> _list;
	private int _version;

	/// <summary>
	/// Gets the number of elements contained in the list.
	/// </summary>
	public int Count
	{
		get
		{
			using (this.EnterScope())
			{
				return _list.Count;
			}
		}
	}
	/// <inheritdoc/>
	bool ICollection<T>.IsReadOnly => false;
	/// <summary>
	/// Gets a value indicating whether the current instance is safe for use by multiple threads concurrently.
	/// </summary>
	/// <remarks>Use this property to determine if operations on the instance can be performed from multiple threads
	/// without additional synchronization. If <see langword="true"/>, the instance can be accessed safely from multiple
	/// threads; otherwise, <see langword="false"/>.  Once a list is marked as not thread-safe via <see cref="MarkNotThreadSafe"/>,
	/// this property will always return <see langword="false"/>.</remarks>
	public bool IsThreadSafe => Volatile.Read(ref _mode) == 0;
	/// <summary>
	/// Gets or sets the element at the specified index within the collection.
	/// </summary>
	/// <param name="index">The zero-based index of the element to get or set. Must be greater than or equal to 0 and less than the number of
	/// elements in the collection.</param>
	/// <returns>The element at the specified index.</returns>
	public T this[int index]
	{
		get
		{
			using (this.EnterScope())
			{
				return _list[index];
			}
		}
		set
		{
			using (this.EnterScope())
			{
				this.IncrementVersion();
				_list[index] = value;
			}
		}
	}
	/// <summary>
	/// Initializes a new instance of the SyncList class with the specified initial capacity.
	/// </summary>
	/// <param name="capacity">The number of elements that the list can initially contain. Must be non-negative.</param>
	public SyncList(int capacity)
	{
		_list = new(capacity);
		_mode = 0;
	}
	/// <summary>
	/// Initializes a new instance of the SyncList class with the specified values.
	/// </summary>
	/// <param name="values">A read-only span containing the values to initialize the list with. The contents of the span are copied into the
	/// new list.</param>
	internal SyncList(params ReadOnlySpan<T> values)
	{
		_list = [.. values];
		_mode = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SyncList{T}"/> class using the specified list as the underlying data source.
	/// </summary>
	/// <param name="list">The list to be used as the underlying data source. Cannot be null.</param>
	/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
	internal SyncList(List<T> list)
	{
		ArgumentNullException.ThrowIfNull(list);
		_list = list;
		_mode = 0;
	}

	/// <summary>
	/// Adds the specified item to the end of the list.
	/// </summary>
	/// <param name="item">The item to add to the collection.</param>
	public void Add(T item)
	{
		using (this.EnterScope())
		{
			this.IncrementVersion();
			this.AddCore(item, _list.Count);
		}
	}
	/// <summary>
	/// Adds the elements of the specified collection to the end of the list.
	/// </summary>
	/// <remarks>If the specified collection is a <see cref="List{T}"/> or an array, this method adds the elements
	/// efficiently using a span-based operation. The order of the added elements is preserved. This method increases the
	/// version of the list, which may invalidate any active enumerators.</remarks>
	/// <param name="items">The collection whose elements should be added to the end of the list. Cannot be null.</param>
	public void AddRange(IEnumerable<T> items)
	{
		if (items is List<T> list)
		{
			this.AddRange(
				items: new ReadOnlySpan<T>(ListMarshal.GetBackingArray(list)));

			return;
		}
		else if (items is T[] array)
		{
			this.AddRange(new ReadOnlySpan<T>(array));
			return;
		}
		else if (items is IReadOnlyArray<T> roArray)
		{
			this.AddRange(roArray.AsSpan());
			return;
		}

		using (this.EnterScope())
		{
			this.IncrementVersion();
			this.AddRangeCore(items, _list.Count);
		}
	}
	/// <summary>
	/// Adds the elements of the specified read-only span to the end of the collection.
	/// </summary>
	/// <param name="items">A read-only span containing the elements to add. If the span is empty, no elements are added.</param>
	public void AddRange(params ReadOnlySpan<T> items)
	{
		if (items.IsEmpty)
			return;

		using (this.EnterScope())
		{
			this.IncrementVersion();
			this.AddRangeCore(items, _list.Count);
		}
	}

	/// <summary>
	/// Adds elements to the collection by deserializing each item in the specified JSON array.
	/// </summary>
	/// <remarks>Only items in the array that can be successfully deserialized to the target type are added. Items
	/// that fail deserialization are skipped.</remarks>
	/// <param name="array">The JSON array containing elements to be deserialized and added to the collection. Must be of type <see
	/// cref="JsonValueKind.Array"/>.</param>
	/// <param name="options">Optional serializer options to control the deserialization behavior. If <see langword="null"/>, default options are
	/// used.</param>
	/// <returns>The number of items successfully added to the collection.</returns>
	public int AddFromJsonArray(JsonElement array, JsonSerializerOptions? options)
	{
		Debug.Assert(array.ValueKind == JsonValueKind.Array);
		int addedCount = 0;
		using (this.EnterScope())
		{
			this.IncrementVersion();
			this.EnsureCapacity(array.GetArrayLength() + _list.Count);

			foreach (JsonElement item in array.EnumerateArray())
			{
				if (item.Deserialize<T>(options) is T value)
				{
					addedCount++;
					this.AddCore(value, _list.Count);
				}
			}
		}

		return addedCount;
	}
	/// <summary>
	/// Adds the specified item to the list.
	/// </summary>
	/// <remarks>
	/// Derived classes can override this method to customize the behavior of adding items to the collection.
	/// When this list is in thread-safe mode, this method is called from within a lock to ensure thread safety.
	/// <para>
	/// Default implementation simply adds the item to the underlying list.
	/// </para>
	/// </remarks>
	/// <param name="item">The item to add to the collection.</param>
	/// <param name="currentCount">The current count of items in the collection before adding the new item.</param>
	protected virtual void AddCore(T item, int currentCount)
	{
		_list.Add(item);
	}
	/// <summary>
	/// Adds the specified items to the underlying collection.
	/// </summary>
	/// <param name="items">A read-only span containing the items to add to the collection.</param>
	/// <param name="currentCount">The current number of items in the collection before the new items are added.</param>
	protected virtual void AddRangeCore(ReadOnlySpan<T> items, int currentCount)
	{
		_list.AddRange(items);
	}
	/// <summary>
	/// Adds the specified collection of items to the underlying list.
	/// </summary>
	/// <param name="items">The collection of items to add to the list. Cannot be null.</param>
	/// <param name="currentCount">The current number of items in the list before the new items are added.</param>
	protected virtual void AddRangeCore(IEnumerable<T> items, int currentCount)
	{
		_list.AddRange(items);
	}
	/// <summary>
	/// Removes all items from the collection.
	/// </summary>
	/// <remarks>After calling this method, the collection will be empty. This operation may affect any observers or
	/// listeners that depend on the collection's contents.</remarks>
	public void Clear()
	{
		using (this.EnterScope())
		{
			this.IncrementVersion();
			this.ClearCore(_list.Count);
		}
	}
	/// <summary>
	/// Removes all items from the underlying collection.
	/// </summary>
	/// <param name="currentCount">The number of items currently in the collection before clearing. This value is informational and does not affect
	/// the clearing operation.</param>
	protected virtual void ClearCore(int currentCount)
	{
		_list.Clear();
	}

	/// <summary>
	/// Determines whether the list contains a specific element.
	/// </summary>
	/// <param name="item">The element to locate in the list.</param>
	/// <returns>true if the specified element is found in the list; otherwise, false.</returns>
	public bool Contains(T item)
	{
		using (this.EnterScope())
		{
			return this.ContainsCore(item, _list.Count);
		}
	}
	/// <summary>
	/// Determines whether the specified item exists in the collection.
	/// </summary>
	/// <remarks>Derived classes can override this method to customize how containment is determined, potentially
	/// using the currentCount parameter to affect the result.</remarks>
	/// <param name="item">The item to locate in the collection. The item is compared using the collection's equality comparer.</param>
	/// <param name="currentCount">The current number of items in the collection. This value may be used by overrides to influence containment logic.</param>
	/// <returns>true if the item is found in the collection; otherwise, false.</returns>
	protected virtual bool ContainsCore(T item, int currentCount)
	{
		return _list.Contains(item);
	}
	/// <summary>
	/// Creates a new list containing all elements of the current list, cast to the specified reference type.
	/// </summary>
	/// <remarks>All elements are cast using a direct reference cast. An InvalidCastException will be thrown if any
	/// element cannot be cast to the specified type. The returned list has the same number of elements as the
	/// original.</remarks>
	/// <typeparam name="TOther">The reference type to which each element in the list will be cast.</typeparam>
	/// <returns>A new SyncList containing all elements of the current list, cast to type TOther.</returns>
	/// <exception cref="InvalidCastException"></exception>
	public SyncList<TOther> CastAll<TOther>() where TOther : class
	{
		using (this.EnterScope())
		{
			int count = _list.Count;
			SyncList<TOther> newList = new(count);
			ByRefTuple<T, TOther> tuple = ListMarshal.GetTwoArrayFirstRef(_list, newList._list, count);

			for (int i = 0; i < count; i++)
			{
				Unsafe.Add(ref tuple.Item2, i) = (TOther)(object)Unsafe.Add(ref tuple.Item1, i)!;
			}

			ListMarshal.SetCount(newList._list, count);
			Debug.Assert(newList._list.Count == _list.Count);
			return newList;
		}
	}

	/// <summary>
	/// Creates a new list by converting each element of the current list to a new type using the specified converter
	/// function.
	/// </summary>
	/// <remarks>The conversion is performed in-place for performance. The returned list is independent of the
	/// original and modifications to one do not affect the other.</remarks>
	/// <typeparam name="TOther">The type of the elements in the new list resulting from the conversion.</typeparam>
	/// <param name="converter">A pointer to a function that converts an element of type <typeparamref name="T"/> to <typeparamref name="TOther"/>.</param>
	/// <returns>A new SyncList containing the converted elements. The order and count of elements match the original list.</returns>
	/// <exception cref="InvalidOperationException"><paramref name="converter"/>'s pointer is null.</exception>
	public unsafe SyncList<TOther> ConvertAll<TOther>(FnPtr<T, TOther> converter)
	{
		FnPtr.ThrowIfNull(converter);

		int count;
		SyncList<TOther> newList;
		using (this.EnterScope())
		{
			count = _list.Count;
			newList = new(count);
			ByRefTuple<T, TOther> tuple = ListMarshal.GetTwoArrayFirstRef(_list, newList._list, count);

			for (int i = 0; i < count; i++)
			{
				Unsafe.Add(ref tuple.Item2, i) = converter.Invoke(Unsafe.Add(ref tuple.Item1, i));
			}
		}

		ListMarshal.SetCount(newList._list, count);
		Debug.Assert(newList._list.Count == _list.Count);
		return newList;
	}
	/// <summary>
	/// Creates a new list by converting each element of the current list to a new type using the specified converter
	/// function.
	/// </summary>
	/// <remarks>The conversion is performed in-place for performance. The returned list is independent of the
	/// original and modifications to one do not affect the other.</remarks>
	/// <typeparam name="TOther">The type of the elements in the new list resulting from the conversion.</typeparam>
	/// <param name="converter">A pointer to a function that converts an element of type <typeparamref name="T"/> to <typeparamref name="TOther"/>.</param>
	/// <returns>A new SyncList containing the converted elements. The order and count of elements match the original list.</returns>
	/// <exception cref="InvalidOperationException"><paramref name="converter"/>'s pointer is null.</exception>
	public SyncList<TOther> ConvertAll<TOther, TState>(TState state, FnPtr<T, TState, TOther> converter) where TState : allows ref struct
	{
		FnPtr.ThrowIfNull(converter);

		int count;
		SyncList<TOther> newList;
		using (this.EnterScope())
		{
			count = _list.Count;
			newList = new(count);
			ByRefTuple<T, TOther> tuple = ListMarshal.GetTwoArrayFirstRef(_list, newList._list, count);

			for (int i = 0; i < count; i++)
			{
				Unsafe.Add(ref tuple.Item2, i) = converter.Invoke(Unsafe.Add(ref tuple.Item1, i), state);
			}
		}

		ListMarshal.SetCount(newList._list, count);
		Debug.Assert(newList._list.Count == count);
		return newList;
	}

	/// <summary>
	/// Copies the elements of the collection to the specified array, starting at the given array index.
	/// </summary>
	/// <remarks>The destination array must be large enough to contain all the elements from the collection,
	/// starting at the specified index. This method performs a shallow copy of the elements. If the array is null, or if
	/// arrayIndex is less than zero, or if there is insufficient space from arrayIndex to the end of the array, an
	/// exception will be thrown.</remarks>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have
	/// zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in the destination array at which copying begins.</param>
	public void CopyTo(T[] array, int arrayIndex)
	{
		using (this.EnterScope())
		{
			_list.CopyTo(array, arrayIndex);
		}
	}
	/// <summary>
	/// Ensures that the underlying list can hold at least the specified number of elements without resizing.
	/// </summary>
	/// <remarks>If the current capacity is already greater than or equal to the specified value, no change is made.
	/// Otherwise, the capacity is increased. This method may improve performance by reducing the number of reallocations
	/// required when adding elements.</remarks>
	/// <param name="capacity">The minimum number of elements that the underlying list should be able to contain. Must be non-negative.</param>
	/// <returns>The new capacity of the underlying list after the operation, which is at least the specified value.</returns>
	public int EnsureCapacity(int capacity)
	{
		using (this.EnterScope())
		{
			this.IncrementVersion();
			return _list.EnsureCapacity(capacity);
		}
	}

	/// <summary>
	/// Returns the zero-based index of the first occurrence of the specified item within the collection.
	/// </summary>
	/// <param name="item">The item to locate in the collection. The value can be null for reference types.</param>
	/// <returns>The zero-based index of the first occurrence of the specified item if found; otherwise, –1.</returns>
	public int IndexOf(T item)
	{
		using (this.EnterScope())
		{
			return this.IndexOfCore(item, _list.Count);
		}
	}
	/// <summary>
	/// Searches for the specified item and returns the zero-based index of its first occurrence within the collection.
	/// </summary>
	/// <remarks>Derived classes can override this method to customize how the search is performed, such as
	/// restricting the search to a subset of the collection using the currentCount parameter.</remarks>
	/// <param name="item">The item to locate in the collection. The value can be null for reference types.</param>
	/// <param name="currentCount">The number of items currently in the collection. This parameter may be used by derived classes to limit the search
	/// range.</param>
	/// <returns>The zero-based index of the first occurrence of the specified item within the collection, if found; otherwise, –1.</returns>
	protected virtual int IndexOfCore(T item, int currentCount)
	{
		return _list.IndexOf(item);
	}
	/// <summary>
	/// Inserts the specified item into the collection at the given index.
	/// </summary>
	/// <param name="index">The zero-based index at which the item should be inserted. Must be greater than or equal to 0 and less than or
	/// equal to the number of items in the collection.</param>
	/// <param name="item">The item to insert into the collection.</param>
	public void Insert(int index, T item)
	{
		using (this.EnterScope())
		{
			this.IncrementVersion();
			this.InsertCore(index, item, _list.Count);
		}
	}
	/// <summary>
	/// Inserts the specified item into the collection at the given index.
	/// </summary>
	/// <param name="index">The zero-based index at which the item should be inserted. Must be greater than or equal to 0 and less than or
	/// equal to the current number of items.</param>
	/// <param name="item">The item to insert into the collection.</param>
	/// <param name="currentCount">The current number of items in the collection before insertion.</param>
	protected virtual void InsertCore(int index, T item, int currentCount)
	{
		_list.Insert(index, item);
	}

	/// <summary>
	/// Permanently disables locking. Blocks until any in-flight critical sections complete.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if the list was previously thread-safe and is now marked as not thread-safe;
	/// otherwise, <see langword="false"/> if it was already marked as not thread-safe.
	/// </returns>
	public bool MarkNotThreadSafe()
	{
		if (Volatile.Read(ref _mode) == 1)
			return false;

		// Quiesce writers/readers currently in the lock.
		using (_lock.EnterScope())
		{
			this.IncrementVersion();
			// Publish: after this write, future operations observe NotThreadSafe.
			Volatile.Write(ref _mode, 1);
			return true;
		}
	}
	/// <summary>
	/// Removes the first occurrence of the specified item from the collection.
	/// </summary>
	/// <param name="item">The item to remove from the collection. If the item is not found, no action is taken.</param>
	/// <returns>true if the item was successfully removed; otherwise, false.</returns>
	public bool Remove(T item)
	{
		using (this.EnterScope())
		{
			this.IncrementVersion();
			return this.RemoveCore(item, _list.Count);
		}
	}
	/// <summary>
	/// Removes the specified item from the collection.
	/// </summary>
	/// <remarks>Derived classes can override this method to customize the removal behavior. The default
	/// implementation removes the item from the underlying list.</remarks>
	/// <param name="item">The item to remove from the collection.</param>
	/// <param name="currentCount">The current number of items in the collection before removal. This value may be used by derived classes to
	/// implement custom removal logic.</param>
	/// <returns>true if the item was successfully removed; otherwise, false.</returns>
	protected virtual bool RemoveCore(T item, int currentCount)
	{
		return _list.Remove(item);
	}
	/// <summary>
	/// Removes the element at the specified index from the collection.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove. Must be greater than or equal to 0 and less than the number of
	/// elements in the collection.</param>
	public void RemoveAt(int index)
	{
		using (this.EnterScope())
		{
			this.IncrementVersion();
			this.RemoveAtCore(index, _list.Count);
		}
	}
	/// <summary>
	/// Removes the element at the specified index from the underlying list.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove. Must be greater than or equal to 0 and less than the current count.</param>
	/// <param name="currentCount">The current number of elements in the list before removal. Used to validate the index range.</param>
	protected virtual void RemoveAtCore(int index, int currentCount)
	{
		_list.RemoveAt(index);
	}
	/// <summary>
	/// Sorts the elements in the entire collection using the default comparer.
	/// </summary>
	/// <remarks>This method uses the default comparer to determine the order of elements. All elements in the
	/// collection must implement the IComparable interface or a runtime exception will occur. To specify a custom
	/// comparison, use the overload that accepts an <see cref="IComparer{T}"/> parameter.</remarks>
	public void Sort()
	{
		this.Sort(comparer: null);
	}
	/// <summary>
	/// Sorts the elements in the collection using the specified comparer.
	/// </summary>
	/// <param name="comparer">The comparer to use when comparing elements, or null to use the default comparer for the element type.</param>
	public void Sort(IComparer<T>? comparer)
	{
		using (this.EnterScope())
		{
			this.IncrementVersion();
			_list.Sort(comparer);
		}
	}

	// AND CHAINING METHODS
	/// <summary>
	/// Marks the <see cref="SyncList{T}"/> instance as not thread-safe and returns the same instance.
	/// </summary>
	/// <remarks>After calling this method, the specified SyncList instance should not be accessed from multiple
	/// threads concurrently. Use this method only when thread safety is not required and you want to avoid the overhead of
	/// synchronization.
	/// <para>
	/// <inheritdoc cref="MarkNotThreadSafe" path="/summary"/>
	/// </para>
	/// </remarks>
	/// <returns>The same <see cref="SyncList{T}"/> instance after it has been marked as not thread-safe.</returns>
	public SyncList<T> AndMarkNotThreadSafe()
	{
		_ = this.MarkNotThreadSafe();
		return this;
	}

	/// <summary>
	/// Enters a locking scope if the list is in thread-safe mode.
	/// </summary>
	/// <remarks>
	/// Disposing the returned <see cref="ConditionalLock"/> will exit the locking scope if one was entered. If
	/// the list is not thread-safe, no locking is performed and the returned instance will be a default - which is
	/// still okay to dispose.
	/// </remarks>
	/// <returns>
	/// A <see cref="ConditionalLock"/> that represents the locking scope. If the list is not thread-safe, a default
	/// instance is returned.
	/// </returns>
	private ConditionalLock EnterScope()
	{
		if (Volatile.Read(ref _mode) == 0)
		{
			var scope = _lock.EnterScope();
			return new ConditionalLock(ref scope);
		}

		return default;
	}
	private void IncrementVersion()
	{
		int newVersion = Volatile.Read(ref _version) + 1;
		Volatile.Write(ref _version, newVersion);
	}
}

file static class ListMarshal
{
	/// <summary>
	/// Initializes static data for the ListMarshal class and verifies the internal structure of the generic <see cref="List{T}"/> type.
	/// </summary>
	/// <remarks>This static constructor inspects the private fields of <see cref="List{T}"/> to ensure compatibility with its
	/// internal representation. If the structure of <see cref="List{T}"/> changes in future .NET versions, this check helps prevent
	/// incorrect or unsafe operations that rely on the current layout.</remarks>
	/// <exception cref="InvalidOperationException">Thrown if the internal structure of <see cref="List{T}"/> does not match the expected layout, indicating that the implementation
	/// has changed and the class cannot operate safely.</exception>
	static ListMarshal()
	{
		FieldInfo[] fields = typeof(List<>).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
		if (!(fields.Length == 3
			&& fields[0].Name == "_items" && fields[0].FieldType.IsArray
			&& fields[1].Name == "_size" && typeof(int).Equals(fields[1].FieldType)
			&& fields[2].Name == "_version") && typeof(int).Equals(fields[2].FieldType))
		{
			throw new InvalidOperationException("List has changed its internal structure; cannot continue.");
		}
	}

	internal static ByRefTuple<T, TOther> GetTwoArrayFirstRef<T, TOther>(List<T> source, List<TOther> target, int count)
	{
		Debug.Assert(count == source.Count, "The count is intended to match the source list count.");
		_ = target.EnsureCapacity(count);

		ref T fOfT = ref MemoryMarshal.GetArrayDataReference(GetBackingArray(source));
		ref TOther fOfTOther = ref MemoryMarshal.GetArrayDataReference(GetBackingArray(target));
		return new(ref fOfT, ref fOfTOther);
	}

	internal static void SetCount<T>(List<T> list, int size)
	{
		ListView<T> view = Unsafe.As<ListView<T>>(list);
		Debug.Assert(size <= view._items.Length, "The size being set should be less than or equal to the array length.");
		view._size = size;
	}

	internal static T[] GetBackingArray<T>(List<T> list)
	{
		Debug.Assert(list is not null, "The list parameter should not be null.");
		return Unsafe.As<ListView<T>>(list)._items;
	}

	private sealed class ListView<T>
	{
		internal T[] _items = null!;
		internal int _size;
		[SuppressMessage("Style", "IDE0044")]
		[SuppressMessage("Style", "IDE0051")]
#pragma warning disable 0169
		private int _version;
#pragma warning restore
	}
}