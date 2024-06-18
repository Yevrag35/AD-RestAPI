using AD.Api.Core.Settings;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.Versioning;

namespace AD.Api.Core.Schema
{
    public sealed class SchemaClassPropertyDictionary : IReadOnlyDictionary<string, SchemaProperty>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly FrozenDictionary<string, SchemaProperty> _dict;

        /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.this[TKey]"/>/>
        public ref readonly SchemaProperty this[string key]
        {
            [DebuggerStepThrough]
            get => ref _dict[key];
        }

        /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.Count"/>
        public int Count
        {
            [DebuggerStepThrough]
            get => _dict.Count;
        }
        public string DomainName { get; }
        public string DomainKey { get; }
        /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.Keys"/>
        public ImmutableArray<string> Keys
        {
            [DebuggerStepThrough]
            get => _dict.Keys;
        }
        /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.Values"/>
        public ImmutableArray<SchemaProperty> Values
        {
            [DebuggerStepThrough]
            get => _dict.Values;
        }

        internal SchemaClassPropertyDictionary(string domainKey, string domainName, IDictionary<string, SchemaProperty> dictionary)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(domainName);
            ArgumentNullException.ThrowIfNull(dictionary);

            _dict = dictionary.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            this.DomainName = domainName;
            this.DomainKey = domainKey;
        }

        [DebuggerStepThrough]
        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }
        [DebuggerStepThrough]
        public IEnumerator<KeyValuePair<string, SchemaProperty>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
        [DebuggerStepThrough]
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out SchemaProperty value)
        {
            return _dict.TryGetValue(key, out value);
        }

        SchemaProperty IReadOnlyDictionary<string, SchemaProperty>.this[string key]
        {
            [DebuggerStepThrough]
            get => this[key];
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<string> IReadOnlyDictionary<string, SchemaProperty>.Keys => this.Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<SchemaProperty> IReadOnlyDictionary<string, SchemaProperty>.Values => this.Values;
        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

