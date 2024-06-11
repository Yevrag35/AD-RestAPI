using AD.Api.Collections.Enumerators;
using System.Collections;
using System.Collections.Frozen;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public sealed class ContextLibrary : IReadOnlyCollection<ConnectionContext>
    {
        private readonly FrozenDictionary<string, ConnectionContext> _dictionary;

        public ref readonly ConnectionContext this[string? key] => ref this.GetValueOrDefault(key);

        public int Count => _dictionary.Count;

        internal ContextLibrary(Dictionary<string, ConnectionContext> dictionary)
        {
            _dictionary = dictionary.ToFrozenDictionary(dictionary.Comparer);
        }

        private ref readonly ConnectionContext GetValueOrDefault(string? key)
        {
            key ??= string.Empty;
            return ref _dictionary[key];
        }
        public IEnumerator<ConnectionContext> GetEnumerator()
        {
            ReadOnlySpan<ConnectionContext> span = _dictionary.Values.AsSpan();
            return new ArrayEnumerator<ConnectionContext>(span);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue([NotNullWhen(false)] string? key, [NotNullWhen(true)] out ConnectionContext? value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = this[string.Empty];
                return true;
            }

            return _dictionary.TryGetValue(key, out value);
        }
    }
}

