using System.Collections;

namespace AD.Api.Collections;

[DebuggerDisplay("Count = {Count}")]
public class UnsafeDictionary<TKey> : IReadOnlyDictionary<TKey, object>
	where TKey : notnull
{
	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	readonly Dictionary<TKey, object> _dict;

	/// <inheritdoc cref="IReadOnlyCollection{T}.Count"/>
	public int Count
	{
		[DebuggerStepThrough]
		get => _dict.Count;
	}
	/// <summary>
	/// Gets a collection containing the keys in the dictionary.
	/// </summary>
	/// <returns>
	/// A <see cref="Dictionary{TKey, TValue}.KeyCollection"/> containing the keys in the dictionary.
	/// </returns>
	protected Dictionary<TKey, object>.KeyCollection Keys
	{
		[DebuggerStepThrough]
		get => _dict.Keys;
	}
	/// <summary>
	/// Gets a collection containing the values in the dictionary.
	/// </summary>
	/// <returns>
	/// A <see cref="Dictionary{TKey, TValue}.ValueCollection"/> containing the values in the dictionary.
	/// </returns>
	protected Dictionary<TKey, object>.ValueCollection Values
	{
		[DebuggerStepThrough]
		get => _dict.Values;
	}

	[DebuggerStepThrough]
	public UnsafeDictionary()
		: this(0, null)
	{
	}
	[DebuggerStepThrough]
	public UnsafeDictionary(int capacity)
		: this(capacity, null)
	{
	}
	[DebuggerStepThrough]
	public UnsafeDictionary(IEqualityComparer<TKey>? keyComparer)
		: this(0, keyComparer)
	{
	}
	public UnsafeDictionary(int capacity, IEqualityComparer<TKey>? keyComparer)
	{
		_dict = new(capacity, keyComparer);
	}

	/// <inheritdoc cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/>
	protected void AddCore<TValue>(TKey key, [DisallowNull] TValue value) where TValue : class
	{
		ArgumentNullException.ThrowIfNull(value);
		_dict.Add(key, value);
	}

	/// <summary>
	/// Removes all keys and values from the dictionary.
	/// </summary>
	[DebuggerStepThrough]
	public void Clear()
	{
		this.ClearItems();
	}
	/// <summary>
	/// Removes all keys and values from the dictionary.
	/// </summary>
	protected virtual void ClearItems()
	{
		_dict.Clear();
	}

	/// <summary>
	/// Determines whether the dictionary contains the specified key.
	/// </summary>
	/// <param name="key">
	///     <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.ContainsKey(TKey)"/>
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the dictionary contains an element with the specified key; otherwise, 
	/// <see langword="false"/>.
	/// </returns>
	[DebuggerStepThrough]
	public bool ContainsKey(TKey key)
	{
		return _dict.ContainsKey(key);
	}
	/// <inheritdoc cref="Dictionary{TKey, TValue}.EnsureCapacity(int)" path="/*[not(self::returns)]"/>
	/// <returns>
	/// The current capacity of the dictionary.
	/// </returns>
	[DebuggerStepThrough]
	public int EnsureCapacity(int capacity)
	{
		return _dict.EnsureCapacity(capacity);
	}
	[DebuggerStepThrough]
	public IEnumerator<KeyValuePair<TKey, object>> GetEnumerator()
	{
		return _dict.GetEnumerator();
	}
	[DebuggerStepThrough]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}
	/// <summary>
	/// Removes the value with the specified key from the dictionary.
	/// </summary>
	/// <param name="key">The key to locate.</param>
	/// <returns>
	/// <see langword="true"/> if the key is successfully found and the value removed; otherwise, 
	/// <see langword="false"/>.
	/// </returns>
	/// <inheritdoc cref="Dictionary{TKey, TValue}.Remove(TKey)" path="/exception"/>
	protected virtual bool RemoveItem(TKey key)
	{
		return _dict.Remove(key);
	}
	/// <inheritdoc cref="Dictionary{TKey, TValue}.TrimExcess()"/>
	[DebuggerStepThrough]
	public void TrimExcess()
	{
		this.TrimExcessCore();
	}
	/// <inheritdoc cref="Dictionary{TKey, TValue}.TrimExcess(int)"/>
	[DebuggerStepThrough]
	public void TrimExcess(int capacity)
	{
		this.TrimExcessCore(capacity);
	}
	/// <inheritdoc cref="Dictionary{TKey, TValue}.TrimExcess()"/>
	protected virtual void TrimExcessCore()
	{
		_dict.TrimExcess();
	}
	/// <inheritdoc cref="Dictionary{TKey, TValue}.TrimExcess(int)"/>
	protected virtual void TrimExcessCore(int capacity)
	{
		_dict.TrimExcess(capacity);
	}


	/// <inheritdoc cref="Dictionary{TKey, TValue}.TryAdd(TKey, TValue)"/>
	[DebuggerStepThrough]
	protected bool TryAddCore<TValue>(TKey key, [DisallowNull] TValue value) where TValue : class
	{
		ArgumentNullException.ThrowIfNull(value);
		return _dict.TryAdd(key, value);
	}

	/// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" path="/*[not(self::returns)]"/>
	/// <returns>
	/// <see langword="true"/> if the dictionary contains an element with the specified key; otherwise,
	/// <see langword="false"/>.
	/// </returns>
	protected virtual bool TryGetValue(TKey key, [NotNullWhen(true)] out object? value)
	{
		return _dict.TryGetValue(key, out value);
	}
	/// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" path="/*[not(self::returns)]"/>
	/// <returns>
	/// <see langword="true"/> if the dictionary contains an element with the specified key; otherwise,
	/// <see langword="false"/>.
	/// </returns>
	public bool TryGetValue<TValue>(TKey key, [NotNullWhen(true)] out TValue? castedValue) where TValue : class?
	{
		if (_dict.TryGetValue(key, out object? obj))
		{
			castedValue = (TValue)obj;
			return true;
		}
		else
		{
			castedValue = null;
			return false;
		}
	}
	public bool TryGetUnsafeValue<TValue>(TKey key, [NotNullWhen(true)] out TValue? unsafeValue) where TValue : class?
	{
		if (_dict.TryGetValue(key, out object? obj))
		{
			unsafeValue = Unsafe.As<TValue>(obj);
			return true;
		}
		else
		{
			unsafeValue = null;
			return false;
		}
	}

	#region DICTIONARY INTERFACE IMPLEMENTATIONS

	/// <inheritdoc/>
	object IReadOnlyDictionary<TKey, object>.this[TKey key]
	{
		[DebuggerStepThrough]
		get => _dict[key];
	}

	/// <inheritdoc/>
	IEnumerable<TKey> IReadOnlyDictionary<TKey, object>.Keys
	{
		[DebuggerStepThrough]
		get => this.Keys;
	}
	/// <inheritdoc/>
	IEnumerable<object> IReadOnlyDictionary<TKey, object>.Values
	{
		[DebuggerStepThrough]
		get => this.Values;
	}

	/// <inheritdoc/>
	[DebuggerStepThrough]
	bool IReadOnlyDictionary<TKey, object>.TryGetValue(TKey key, [NotNullWhen(true)] out object? value)
	{
		return this.TryGetValue(key, out value);
	}

	#endregion
}

