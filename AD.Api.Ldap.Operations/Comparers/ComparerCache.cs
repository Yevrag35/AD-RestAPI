using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace AD.Api.Ldap.Operations
{
    public sealed class ComparerCache : IReadOnlyDictionary<Type, PropertyValueComparer>
    {
        private readonly Dictionary<Type, PropertyValueComparer> _cache;

        public PropertyValueComparer this[Type key] => _cache.TryGetValue(key, out PropertyValueComparer? comparer)
            ? comparer
            : _cache[typeof(string)];

        public int Count => _cache.Count;

        public IEnumerable<Type> Keys => _cache.Keys;
        public IEnumerable<PropertyValueComparer> Values => _cache.Values;

        public ComparerCache()
        {
            _cache = new Dictionary<Type, PropertyValueComparer>(7)
            {
                { typeof(string), new(typeof(string)) },
                { typeof(int), new(typeof(int)) },
                { typeof(uint), new(typeof(uint)) },
                { typeof(long), new(typeof(long)) },
                { typeof(Guid), new(typeof(Guid)) }
            };
        }

        public IEnumerator<KeyValuePair<Type, PropertyValueComparer>> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool ContainsKey(Type? key)
        {
            return !(key is null) && _cache.ContainsKey(key);
        }

        public bool TryGetValue(Type? key, [MaybeNullWhen(false)] out PropertyValueComparer value)
        {
            value = null;
            return !(key is null) && _cache.TryGetValue(key, out value);
        }
    }
}
