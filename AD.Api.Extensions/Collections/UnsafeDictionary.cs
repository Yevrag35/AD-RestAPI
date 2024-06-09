using System.Collections;

namespace AD.Api.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    public class UnsafeDictionary<TKey> : IEnumerable<KeyValuePair<TKey, object>> where TKey : notnull
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly Dictionary<TKey, object> _dict;

        /// <inheritdoc cref="IReadOnlyCollection{T}.Count"/>
        public int Count
        {
            [DebuggerStepThrough]
            get => _dict.Count;
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

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        [DebuggerStepThrough]
        public void Clear()
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
        [DebuggerStepThrough]
        public bool Remove(TKey key)
        {
            return _dict.Remove(key);
        }
        /// <inheritdoc cref="Dictionary{TKey, TValue}.TrimExcess()"/>
        [DebuggerStepThrough]
        public void TrimExcess()
        {
            _dict.TrimExcess();
        }
        /// <inheritdoc cref="Dictionary{TKey, TValue}.TrimExcess(int)"/>
        [DebuggerStepThrough]
        public void TrimExcess(int capacity)
        {
            _dict.TrimExcess(capacity);
        }
        /// <inheritdoc cref="Dictionary{TKey, TValue}.TryAdd(TKey, TValue)"/>
        [DebuggerStepThrough]
        public bool TryAdd<TValue>(TKey key, [DisallowNull] TValue value) where TValue : class
        {
            ArgumentNullException.ThrowIfNull(value);
            return _dict.TryAdd(key, value);
        }
        /// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" path="/*[not(self::returns)]"/>
        /// <returns>
        /// <see langword="true"/> if the dictionary contains an element with the specified key; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetValue<TValue>(TKey key, [NotNullWhen(true)] out TValue? value) where TValue : class
        {
            if (_dict.TryGetValue(key, out object? obj))
            {
                value = (TValue)obj;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}

