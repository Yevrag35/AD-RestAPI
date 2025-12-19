using System.Collections.Concurrent;

namespace AD.Api.Collections;

[DebuggerDisplay("Count = {Count}")]
public sealed class ConcurrentSet<T> : IReadOnlyCollection<T> where T : notnull
{
	private readonly ConcurrentDictionary<T, byte> _dictionary;

	public int Count => _dictionary.Count;

	public ConcurrentSet()
	{
		_dictionary = new ConcurrentDictionary<T, byte>();
	}
	public ConcurrentSet(IEqualityComparer<T>? comparer)
	{
		_dictionary = new ConcurrentDictionary<T, byte>(comparer);
	}
	public ConcurrentSet(int capacity, IEqualityComparer<T>? comparer = null)
	{
		_dictionary = new(-1, capacity, comparer);
	}

	public bool Add(T item)
	{
		return _dictionary.TryAdd(item, default);
	}
	public bool Contains(T item)
	{
		return _dictionary.ContainsKey(item);
	}
	public bool Remove(T item)
	{
		return _dictionary.TryRemove(item, out _);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _dictionary.Keys.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}
}
