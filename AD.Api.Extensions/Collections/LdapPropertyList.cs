using AD.Api.Buffers;
using AD.Api.Collections.Intrinsics;
using ZLinq;

namespace AD.Api.Collections;

/// <summary>
/// Represents a specialized collection of unique string properties that extends <see cref="ArrayList"/>  and provides
/// additional functionality for managing and querying string-based data.
/// </summary>
/// <remarks>This class ensures that all elements in the collection are unique, case-insensitively, and provides 
/// efficient lookup capabilities. It supports operations  such as adding,
/// removing, and querying strings, as well as advanced features like alternate lookups  using <see
/// cref="ReadOnlySpan{T}"/> for performance-critical scenarios. <para> The collection is not thread-safe and should be
/// synchronized externally if accessed concurrently  from multiple threads. </para></remarks>
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(LdapPropertyList), nameof(Create))]
public sealed class LdapPropertyList : ArrayList,
	IList<string>,
	IReadOnlyList<string>,
	IReadOnlySet<string>
{
	private const int MAX_CAPACITY = 50;
	private const int DEFAULT_CAPACITY = 4;
	private readonly HashSet<string> _propSet;
	private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _alternate;

	/// <summary>
	/// Gets or sets the value at the specified index in the collection.
	/// </summary>
	/// <remarks>When setting a value, if the value is not a string, it will be converted to a string using its <see
	/// cref="object.ToString"/> method. If the value is successfully added to the internal property set, it will be stored
	/// in the collection.</remarks>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <returns>The value at the specified index in the collection.</returns>
	/// <inheritdoc cref="ArrayList.this[int]" path="/exception"/>
	/// <exception cref="ArgumentException">The setter value is empty or whitespace.</exception>
	/// <exception cref="ArgumentNullException">The setter value is null.</exception>
	[NotNull]
	public override object? this[int index]
	{
		[DebuggerStepThrough]
		get => base[index] ?? string.Empty;
		set
		{
			if (value is not string strValue)
			{
				strValue = value?.ToString()!;
			}

			ArgumentException.ThrowIfNullOrWhiteSpace(strValue, nameof(value));
			ref string item = ref this.ItemAt(index);
			if (_propSet.Add(strValue))
			{
				_propSet.Remove(item);
				item = strValue;
			}
		}
	}
	/// <inheritdoc/>
	string IList<string>.this[int index]
	{
		[DebuggerStepThrough]
		get => this.ItemAt(index);
		[DebuggerStepThrough]
		set => this[index] = value;
	}
	/// <inheritdoc/>
	string IReadOnlyList<string>.this[int index]
	{
		[DebuggerStepThrough]
		get => this.ItemAt(index);
	}

	/// <summary>
	/// Functionally equivalent to the <c>Count</c> property, but avoids the overhead of 
	/// a virtual call.
	/// </summary>
	public int TotalCount => _propSet.Count;
	public override bool IsFixedSize => false;
	public override bool IsReadOnly => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="LdapPropertyList"/> class with the default capacity.
	/// </summary>
	public LdapPropertyList()
		: base(DEFAULT_CAPACITY)
	{
		_propSet = new(DEFAULT_CAPACITY, StringComparer.OrdinalIgnoreCase);
		_alternate = _propSet.GetAlternateLookup<ReadOnlySpan<char>>();
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="LdapPropertyList"/> class with the specified initial values.
	/// </summary>
	/// <remarks>The <see cref="LdapPropertyList"/> class is initialized with a capacity determined by the provided
	/// <paramref name="initialValues"/>. Duplicate values (case-insensitive) are ignored.</remarks>
	/// <param name="initialValues">A read-only span of strings representing the initial values to populate the property list. Each value is added to
	/// the list using a case-insensitive comparison.</param>
	public LdapPropertyList(params ReadOnlySpan<string> initialValues)
		: base(GetCapacity(initialValues.Length, out int capacity))
	{
		_propSet = new(capacity, StringComparer.OrdinalIgnoreCase);
		this.AddRange(initialValues);
		_alternate = _propSet.GetAlternateLookup<ReadOnlySpan<char>>();
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="LdapPropertyList"/> class by copying the properties from an existing <see
	/// cref="LdapPropertyList"/> instance.
	/// </summary>
	/// <remarks>This constructor creates a new <see cref="LdapPropertyList"/> instance with the same properties as the
	/// specified <paramref name="copyFrom"/> instance. The internal property set is also copied, preserving the comparer
	/// used in the original instance.</remarks>
	/// <param name="copyFrom">The <see cref="LdapPropertyList"/> instance to copy properties from. Must not be <see langword="null"/>.</param>
	private LdapPropertyList(LdapPropertyList copyFrom)
		: base(copyFrom)
	{
		_propSet = new(copyFrom._propSet, copyFrom._propSet.Comparer);
		_alternate = _propSet.GetAlternateLookup<ReadOnlySpan<char>>();
	}

	public override int Add(object? value)
	{
		return value is string strVal && !string.IsNullOrWhiteSpace(strVal)
			? this.AddCore(strVal)
			: -1;
	}
	/// <summary>
	/// Adds the specified value to the collection if it is not null, empty, or whitespace.
	/// </summary>
	/// <param name="value">The string value to add to the collection. Must not be null, empty, or consist only of whitespace.</param>
	/// <returns>The index at which the value was added if the operation was successful; otherwise, -1 if the value is null, empty,
	/// or whitespace.</returns>
	public void Add([DisallowNull] string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);
		_ = this.AddCore(value);
	}
	private int AddCore([DisallowNull] string value)
	{
		return _propSet.Add(value)
			? base.Add(value)
			: -1;
	}
	public override void AddRange(ICollection c)
	{
		if (c is null or { Count: 0 })
		{
			return;
		}

		if (c is IEnumerable<string> strCollection)
		{
			var valueEnum = strCollection.AsValueEnumerable();
			this.AddRangeCore(ref valueEnum);
		}
		else
		{
			var valueEnum = c.AsValueEnumerable().OfType<string>();
			this.AddRangeCore(ref valueEnum);
		}
	}
	/// <summary>
	/// Adds the elements of the specified collection to the current instance.
	/// </summary>
	/// <remarks>This method does not throw an exception if the provided collection is <see langword="null"/>; it
	/// simply performs no operation.</remarks>
	/// <param name="collection">The collection of strings to add. If <paramref name="collection"/> is <see langword="null"/>, no elements are
	/// added.</param>
	public void AddRange(IEnumerable<string> collection)
	{
		if (collection is null)
		{
			return;
		}

		var valueEnum = collection.AsValueEnumerable();
		this.AddRangeCore(ref valueEnum);
	}
	/// <summary>
	/// Adds a range of non-null, non-whitespace strings to the collection.
	/// </summary>
	/// <remarks>Strings that are null or consist only of whitespace are ignored. The method processes the input
	/// using  <see cref="ReadOnlySpan{T}"/> for efficient handling of the provided values.</remarks>
	/// <param name="values">A set of strings to add to the collection. Each string must be non-null and not consist solely of whitespace.</param>
	public void AddRange(params ReadOnlySpan<string> values)
	{
		foreach (string value in values.AsValueEnumerable().Where(x => !string.IsNullOrWhiteSpace(x)))
		{
			this.AddCore(value);
		}
	}
	private void AddRangeCore<TEnumerator>(ref ValueEnumerable<TEnumerator, string> enumerable) where TEnumerator : struct, IValueEnumerator<string>, allows ref struct
	{
		foreach (string item in enumerable.Where(x => !string.IsNullOrWhiteSpace(x)))
		{
			this.AddCore(item);
		}
	}

	public ReadOnlyMemory<string> AsMemory()
	{
		return ArrayListMarshal.AsMemory<string>(this);
	}
	public ReadOnlySpan<string> AsSpan()
	{
		return ArrayListMarshal.AsSpan<string>(this);
	}

	public override void Clear()
	{
		_propSet.Clear();
		base.Clear();
	}
	private LdapPropertyList CloneCore()
	{
		return new(this);
	}
	public override object Clone()
	{
		return this.CloneCore();
	}
	public override bool Contains(object? item)
	{
		return item is string strItem && this.Contains(strItem);
	}
	public bool Contains(string value)
	{
		return _propSet.Contains(value);
	}
	public bool Contains(ReadOnlySpan<char> value)
	{
		return _alternate.Contains(value);
	}
	public int CopyTo(Span<string> buffer, bool sortPrior = false)
	{
		if (sortPrior)
		{
			Array.Sort(ArrayListMarshal.AsRawArray(this), StringComparer.OrdinalIgnoreCase);
		}

		this.AsSpan().CopyTo(buffer);

		return this.Count;
	}
	public void CopyTo(string[] array, int arrayIndex)
	{
		this.AsSpan().CopyTo(array.AsSpan(arrayIndex));
	}
	/// <summary>
	/// Searches for the first element in the collection that matches the conditions defined by the specified predicate.
	/// </summary>
	/// <param name="match">A delegate that defines the conditions of the element to search for. The predicate must not be <see
	/// langword="null"/>.</param>
	/// <returns>The zero-based index of the first element that matches the conditions defined by <paramref name="match"/>,  or -1
	/// if no such element is found.</returns>
	public int FindIndex(Predicate<string> match)
	{
		Debug.Assert(match is not null, "The match predicate must not be null.");
		if (this.Count == 0)
			return -1;

		string[] array = ArrayListMarshal.AsArray<string>(this, out int length);
		return Array.FindIndex(array, 0, length, match);
	}
	/// <summary>
	/// Finds the index of the first element in the collection that matches the specified condition.
	/// </summary>
	/// <remarks>This method uses a delegate pointer to evaluate each element in the collection. Ensure that the 
	/// <paramref name="match"/> function is safe to use in an unsafe context and does not cause undefined
	/// behavior.</remarks>
	/// <param name="match">A pointer to a function that defines the condition to match. The function takes a string as input and returns <see
	/// langword="true"/> if the element satisfies the condition; otherwise, <see langword="false"/>.</param>
	/// <returns>The zero-based index of the first element that matches the condition defined by <paramref name="match"/>,  or -1 if
	/// no matching element is found.</returns>
	public int FindIndex(FnPtr<string, bool> match)
	{
		ReadOnlySpan<string> span = ArrayListMarshal.AsSpan<string>(this);

		for (int i = 0; i < span.Length; i++)
		{
			if (match.Invoke(span[i]))
			{
				return i;
			}
		}

		return -1;
	}

	/// <summary>
	/// Retrieves the item at the specified index in the collection.
	/// </summary>
	/// <param name="index">The zero-based index of the item to retrieve.</param>
	/// <returns>The item at the specified index as a <see cref="string"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException"/>
	private ref string ItemAt(int index)
	{
		return ref ArrayListMarshal.ItemRef<string>(this, index);
	}

	public override int IndexOf(object? value)
	{
		return value is string strValue
			? this.IndexOf(strValue)
			: -1;
	}
	/// <summary>
	/// Searches for the specified string and returns the zero-based index of its first occurrence.
	/// </summary>
	/// <remarks>This method attempts to map the specified string to an internal representation before performing
	/// the search. If the mapping fails, the method returns -1.</remarks>
	/// <param name="value">The string to locate in the collection. Cannot be <see langword="null"/>.</param>
	/// <returns>The zero-based index of the first occurrence of the specified string if found; otherwise, -1.</returns>
	public int IndexOf(string value)
	{
		return _propSet.TryGetValue(value, out string? realValue)
			? base.IndexOf(realValue)
			: -1;
	}
	/// <summary>
	/// Searches for the specified substring within the current instance and returns the zero-based index of its first
	/// occurrence.
	/// </summary>
	/// <remarks>This method attempts to locate the specified substring by first resolving it to a corresponding
	/// value, if available. If the substring cannot be resolved, the method returns -1.</remarks>
	/// <param name="value">The substring to locate within the current instance. This parameter cannot be empty.</param>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value"/> if found; otherwise, -1.</returns>
	public int IndexOf(ReadOnlySpan<char> value)
	{
		return this.TryGetValue(value, out string? realValue)
			? base.IndexOf(realValue)
			: -1;
	}
	public override int IndexOf(object? value, int startIndex)
	{
		return value is string strValue && _propSet.TryGetValue(strValue, out string? realValue)
			? base.IndexOf(realValue, startIndex)
			: -1;
	}
	public override int IndexOf(object? value, int startIndex, int count)
	{
		return value is string strValue && _propSet.TryGetValue(strValue, out string? realValue)
			? base.IndexOf(realValue, startIndex, count)
			: -1;
	}
	public override void Insert(int index, object? value)
	{
		if (value is string strValue)
		{
			this.Insert(index, strValue);
		}
	}
	/// <summary>
	/// Inserts the specified string at the given index in the collection.
	/// </summary>
	/// <remarks>This method ensures that the provided string is valid before attempting the insertion.</remarks>
	/// <param name="index">The zero-based index at which the string should be inserted.</param>
	/// <param name="value">The string to insert. Must not be null, empty, or consist only of whitespace.</param>
	public void Insert(int index, string value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			_ = this.InsertCore(index, value);
		}
	}
	public override void InsertRange(int index, ICollection c)
	{
		if (c is null || c.Count == 0)
		{
			return;
		}

		IEnumerable<string> propsToAdd = ExtractStringEnumerable(c);

		this.InsertRange(index, propsToAdd);
	}
	/// <summary>
	/// Inserts a range of non-null, non-whitespace strings into the collection at the specified index.
	/// </summary>
	/// <remarks>This method ensures that only valid strings (non-null, non-whitespace) are inserted into the
	/// collection. If the specified index is equal to the current count of the collection, the elements are appended to
	/// the end. Otherwise, the elements are inserted starting at the specified index, and the index is incremented for
	/// each successfully inserted element.</remarks>
	/// <param name="index">The zero-based index at which the new elements should be inserted. If the index is equal to the current count of
	/// the collection, the elements are added to the end.</param>
	/// <param name="collection">The collection of strings to insert. Strings that are null, empty, or consist only of whitespace are ignored.</param>
	public void InsertRange(int index, IEnumerable<string> collection)
	{
		if (index == this.Count)
		{
			this.AddRange(collection);
			return;
		}

		foreach (string item in collection.Where(x => !string.IsNullOrWhiteSpace(x)))
		{
			bool added = this.InsertCore(index, item);
			if (added)
				index++;
		}
	}
	/// <summary>
	/// Inserts the specified value at the given index if the value is not already present in the collection.
	/// </summary>
	/// <remarks>This method ensures that duplicate values are not added to the collection. If the value already
	/// exists, the method does not modify the collection and returns <see langword="false"/>.</remarks>
	/// <param name="index">The zero-based index at which the value should be inserted.</param>
	/// <param name="value">The value to insert into the collection. Must not be null.</param>
	/// <returns><see langword="true"/> if the value was successfully inserted; otherwise, <see langword="false"/> if the value
	/// already exists in the collection.</returns>
	private bool InsertCore(int index, string value)
	{
		bool result = false;
		if (_propSet.Add(value))
		{
			base.Insert(index, value);
			result = true;
		}

		return result;
	}
	public override int LastIndexOf(object? value)
	{
		return value is string strValue && _propSet.TryGetValue(strValue, out string? realValue)
			? base.LastIndexOf(realValue)
			: -1;
	}
	public override int LastIndexOf(object? value, int startIndex)
	{
		return value is string strVal && _propSet.TryGetValue(strVal, out string? realValue)
			? base.LastIndexOf(realValue, startIndex)
			: -1;
	}
	public override int LastIndexOf(object? value, int startIndex, int count)
	{
		return value is string strValue && _propSet.TryGetValue(strValue, out string? realValue)
			? base.LastIndexOf(realValue, startIndex, count)
			: -1;
	}
	public override void Remove(object? obj)
	{
		if (obj is string strValue)
		{
			_ = this.Remove(strValue);
		}
	}
	/// <summary>
	/// Removes the specified value from the collection.
	/// </summary>
	/// <remarks>If the specified value exists in the collection, it is removed.  No action is taken if the value
	/// does not exist.</remarks>
	/// <param name="value">The value to be removed from the collection. Must not be null.</param>
	public bool Remove(string value)
	{
		bool result = false;
		if (_propSet.TryGetValue(value, out string? actualValue))
		{
			_ = _propSet.Remove(actualValue);
			base.Remove(actualValue);
			result = true;
		}

		return result;
	}
	/// <summary>
	/// Removes all elements in the specified collection from the current instance.
	/// </summary>
	/// <remarks>Each element in the specified collection is individually removed from the current instance.  If an
	/// element does not exist in the current instance, it is ignored.</remarks>
	/// <param name="collection">A collection of strings to be removed. If the collection is <see langword="null"/>, the method performs no action.</param>
	public void RemoveAll(IEnumerable<string> collection)
	{
		if (collection is null)
		{
			return;
		}

		foreach (string item in collection)
		{
			_ = this.Remove(item);
		}
	}
	/// <summary>
	/// Removes all occurrences of the specified values from the collection.
	/// </summary>
	/// <remarks>This method iterates through the provided values and removes each one from the collection. If a
	/// value does not exist in the collection, it is ignored.</remarks>
	/// <param name="values">An array of strings to be removed from the collection. Each value must not be null.</param>
	internal void RemoveAll(params ReadOnlySpan<string> values)
	{
		foreach (string value in values)
		{
			this.Remove(value);
		}
	}
	public override void RemoveAt(int index)
	{
		string prop = this.ItemAt(index);
		_ = _propSet.Remove(prop);
		base.RemoveAt(index);
	}
	public override void RemoveRange(int index, int count)
	{
		for (int i = index + count - 1; i >= index; i--)
		{
			string prop = this.ItemAt(i);
			_propSet.Remove(prop);
			base.RemoveAt(index);
		}
	}
	public override void SetRange(int index, ICollection c)
	{
		var collection = c
			.AsValueEnumerable()
			.OfType<string>()
			.Where(x => !string.IsNullOrWhiteSpace(x));

		int colCount = c.Count;

		int count = 0;
		foreach (string item in collection)
		{
			if (_propSet.Add(item))
			{
				if (index - count < this.Count)
				{
					ref string existingProp = ref this.ItemAt(index - count);
					_propSet.Remove(existingProp);
					existingProp = item;
				}
				else
				{
					base[index - count] = item;
				}

				count++;
			}
		}
	}
	public override void Sort()
	{
		this.Sort(StringComparer.OrdinalIgnoreCase);
	}
	/// <summary>
	/// Returns a string representation of the collection, with elements separated by a comma and a space.
	/// </summary>
	/// <returns>A string containing the elements of the collection, separated by a comma and a space. If the collection is empty,
	/// returns an empty string.</returns>
	public override string ToString()
	{
		return string.Join(", ", this.AsSpan());
	}
	public override object[] ToArray()
	{
		return base.ToArray()!;
	}
	/// <summary>
	/// Converts the collection to an array of strings.
	/// </summary>
	/// <returns>An array of strings representing the items in the collection.  Returns an empty array if the collection contains no
	/// items.</returns>
	internal string[] ToStringArray()
	{
		return this.Count != 0
			? this.AsSpan().ToArray()
			: [];
	}
	public override void TrimToSize()
	{
		_propSet.TrimExcess();
		base.TrimToSize();
	}
	/// <summary>
	/// Attempts to retrieve the index of the specified value within the collection.
	/// </summary>
	/// <param name="value">The value to locate within the collection, represented as a <see cref="ReadOnlySpan{T}"/> of characters.</param>
	/// <param name="index">When this method returns, contains the zero-based index of the specified value if found;  otherwise, -1. This
	/// parameter is passed uninitialized.</param>
	/// <returns><see langword="true"/> if the specified value is found in the collection; otherwise, <see langword="false"/>.</returns>
	public bool TryGetIndex(ReadOnlySpan<char> value, out int index)
	{
		index = this.TryGetValue(value, out string? actualValue)
			? base.IndexOf(actualValue)
			: -1;

		return index != -1;
	}
	/// <summary>
	/// Attempts to retrieve the value associated with the specified property.
	/// </summary>
	/// <param name="property">The property name to look up, represented as a <see cref="ReadOnlySpan{T}"/> of characters.</param>
	/// <param name="actualValue">When this method returns, contains the value associated with the specified property, if the property is found;
	/// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
	/// <returns><see langword="true"/> if the property is found and its value is successfully retrieved; otherwise, <see
	/// langword="false"/>.</returns>
	public bool TryGetValue(ReadOnlySpan<char> property, [NotNullWhen(true)] out string? actualValue)
	{
		return _alternate.TryGetValue(property, out actualValue);
	}

	public new Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
	IEnumerator<string> IEnumerable<string>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>
	/// Extracts an enumerable collection of strings from the specified <see cref="ICollection"/>.
	/// </summary>
	/// <remarks>This method attempts to extract strings from the input collection in the following order: <list
	/// type="bullet"> <item><description>If the input is <see langword="null"/> or an empty collection, an empty
	/// enumerable is returned.</description></item> <item><description>If the input is an <see cref="IEnumerable{T}"/> of
	/// strings, it is returned as-is.</description></item> <item><description>If the input can be converted to a string
	/// array, the converted array is returned.</description></item> <item><description>Otherwise, the method filters the
	/// input collection to include only elements of type <see cref="string"/>.</description></item> </list></remarks>
	/// <param name="c">The input collection to extract strings from. Can be <see langword="null"/> or any type of <see
	/// cref="ICollection"/>.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of strings extracted from the input collection.  Returns an empty enumerable if the
	/// input is <see langword="null"/>, empty, or does not contain any strings.</returns>
	private static IEnumerable<string> ExtractStringEnumerable(ICollection? c)
	{
		switch (c)
		{
			case null:
			case ICollection<string> strCol when strCol.Count == 0:
			case IReadOnlyCollection<string> roStrCol when roStrCol.Count == 0:
			case ICollection justCol when justCol.Count == 0:
				return [];

			case IEnumerable<string> strEnum:
				return strEnum;

			default:
				return c.OfType<string>();
		}
	}

	/// <summary>
	/// Determines the capacity based on the length of the provided array and a default capacity value.
	/// </summary>
	/// <param name="array">A read-only span of strings used to calculate the capacity. The length of this span is considered in the
	/// calculation.</param>
	/// <param name="capacity">When this method returns, contains the calculated capacity, which is the greater of the array's length or a default
	/// capacity value.</param>
	/// <returns>The calculated capacity, which is the same value assigned to the <paramref name="capacity"/> parameter.</returns>
	private static int GetCapacity(int length, out int capacity)
	{
		return capacity = Math.Max(length, DEFAULT_CAPACITY);
	}

	#region IREADONLYSET IMPLEMENTATION
	public bool IsProperSubsetOf(IEnumerable<string> other)
	{
		return _propSet.IsProperSubsetOf(other);
	}
	public bool IsProperSupersetOf(IEnumerable<string> other)
	{
		return _propSet.IsProperSupersetOf(other);
	}
	public bool IsSubsetOf(IEnumerable<string> other)
	{
		return _propSet.IsSubsetOf(other);
	}
	public bool IsSupersetOf(IEnumerable<string> other)
	{
		return _propSet.IsSupersetOf(other);
	}
	public bool Overlaps(IEnumerable<string> other)
	{
		return _propSet.Overlaps(other);
	}
	public bool SetEquals(IEnumerable<string> other)
	{
		return _propSet.SetEquals(other);
	}

	#endregion

	/// <summary>
	/// Enumerates the elements of a <see cref="LdapPropertyList"/> as strings.
	/// </summary>
	/// <remarks>This enumerator provides a read-only, forward-only iteration over the elements of a <see
	/// cref="LdapPropertyList"/>. It implements <see cref="IEnumerator{T}"/> for strings, allowing it to be used in `foreach`
	/// loops and other enumeration contexts.</remarks>
	[DebuggerStepThrough]
	[StructLayout(LayoutKind.Auto)]
	public struct Enumerator : IEnumerator<string>
	{
		private readonly LdapPropertyList _list;
		private int _index;
		private string _current;

		public readonly string Current => _current;
		readonly object? IEnumerator.Current => this.Current;

		/// <summary>
		/// Initializes a new instance of the <see cref="Enumerator"/> class for iterating over the specified <see
		/// cref="LdapPropertyList"/>.
		/// </summary>
		/// <remarks>This constructor sets up the enumerator to start at the initial position, which is before the
		/// first element in the <see cref="LdapPropertyList"/>.</remarks>
		/// <param name="list">The <see cref="LdapPropertyList"/> to enumerate.</param>
		internal Enumerator(LdapPropertyList list)
		{
			_list = list;
			_index = -1;
			_current = null!;
		}

		public bool MoveNext()
		{
			int next = _index + 1;
			if ((uint)next >= (uint)_list.Count)
			{
				_index = _list.Count;
				return false;
			}

			_index = next;
			_current = _list.ItemAt(_index);
			return true;
		}
		readonly void IDisposable.Dispose() { }
		public void Reset()
		{
			_index = -1;
		}
	}

	public static LdapPropertyList Create(params ReadOnlySpan<string> values)
	{
		return new LdapPropertyList(values);
	}
	//bool IResettable.TryReset()
	//{
	//	// Upper‑bound enforcement: act only if we ever grew past MAX_CAPACITY.
	//	if (this.Capacity > MAX_CAPACITY)
	//	{
	//		// Trim live items so we keep at most DEFAULT_CAPACITY elements.
	//		if (this.Count > DEFAULT_CAPACITY)
	//		{
	//			base.RemoveRange(DEFAULT_CAPACITY, this.Count - DEFAULT_CAPACITY);
	//		}

	//		// Allocate a fresh 4‑slot backing array and copy the remaining items.
	//		this.Capacity = DEFAULT_CAPACITY;
	//	}

	//	// Always leave the collection empty.
	//	this.Clear();
	//	return true;
	//}
}
