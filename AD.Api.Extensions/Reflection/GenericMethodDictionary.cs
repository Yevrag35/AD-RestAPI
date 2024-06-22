using System.Collections.Concurrent;

namespace AD.Api.Reflection
{
    /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}" path="/typeparam"/>
    /// <summary>
    /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}"/> The value type is 
    /// <see cref="MethodInfo"/> for the purposes of caching generic reflection invocations.
    /// </summary>
    public class GenericMethodDictionary<TKey> where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, MethodInfo> _dictionary;

        /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}.this[TKey]"/>
        public MethodInfo this[TKey key] => _dictionary[key];

        /// <summary>
        /// The number of key/value pairs contained in the <see cref="GenericMethodDictionary{TKey}"/>.
        /// </summary>
        public int Count => _dictionary.Count;

        // Constructors
        /// <summary>
        ///     <inheritdoc cref="ConcurrentDictionary{TKey, TValue}()" path="/summary/text()[1]"/>
        ///     <see cref="GenericMethodDictionary{TKey}"/>
        ///     <inheritdoc cref="ConcurrentDictionary{TKey, TValue}()" path="/summary/text()[last()]"/>
        /// </summary>
        public GenericMethodDictionary()
            : this(Environment.ProcessorCount, 0)
        {
        }
        /// <summary>
        ///     <inheritdoc cref="ConcurrentDictionary{TKey, TValue}(int, int)" path="/summary/text()[1]"/>
        ///     <see cref="GenericMethodDictionary{TKey}"/>
        ///     <inheritdoc cref="ConcurrentDictionary{TKey, TValue}(int, int)" path="/summary/text()[last()]"/>
        /// </summary>
        public GenericMethodDictionary(int capacity)
            : this(Environment.ProcessorCount, capacity)
        {
        }
        public GenericMethodDictionary(int capacity, IEqualityComparer<TKey>? comparer)
            : this(Environment.ProcessorCount, capacity, comparer)
        {
        }
        /// <summary>
        ///     <inheritdoc cref="ConcurrentDictionary{TKey, TValue}(int, int)" path="/summary/text()[1]"/>
        ///     <see cref="GenericMethodDictionary{TKey}"/>
        ///     <inheritdoc cref="ConcurrentDictionary{TKey, TValue}(int, int)" path="/summary/text()[last()]"/>
        /// </summary>
        public GenericMethodDictionary(int concurrencyLevel, int capacity)
            : this(concurrencyLevel, capacity, EqualityComparer<TKey>.Default)
        {
        }
        public GenericMethodDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey>? keyComparer)
        {
            _dictionary = new(concurrencyLevel, capacity, keyComparer);
        }

        // Methods
        /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}.TryAdd(TKey, TValue)" path="/exception"/>
        /// <summary>
        /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}.TryAdd(TKey, TValue)" path="/summary/text()[1]"/>
        /// <see cref="GenericMethodDictionary{TKey}"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the key/value pair was added to the <see cref="GenericMethodDictionary{TKey}"/>;
        /// otherwise, if the key already exists or <paramref name="value"/> is <see langword="null"/>, 
        /// <see langword="false"/>.
        /// </returns>
        public bool TryAdd(TKey key, MethodInfo? value)
        {
            ArgumentNullException.ThrowIfNull(key);

            return value is not null && _dictionary.TryAdd(key, value);
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" path="/*[not(self::returns)]"/>
        /// <returns>
        /// <see langword="true"/> if <paramref name="key"/> was found in the <see cref="GenericMethodDictionary{TKey}"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGetValue(TKey key, [NotNullWhen(true)] out MethodInfo? value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}

