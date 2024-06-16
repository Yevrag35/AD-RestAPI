using AD.Api.Collections.Enumerators;
using AD.Api.Pooling;
using System.Buffers;
using System.Collections;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Results
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class ResultEntryCollection : IEnumerable<ResultEntry>
    {
        private readonly List<ResultEntry> _entries;
        private readonly ResultEntryPool _pool;

        public int Count => _entries.Count;
        public string Domain { get; set; }

        internal ResultEntryCollection(int capacity, ResultEntryPool pool)
        {
            this.Domain = string.Empty;
            _entries = new(capacity);
            _pool = pool;
        }

        public void Add(SearchResultEntry searchResult)
        {
            ResultEntry entry = _pool.GetItem();
            entry.AddResult(this.Domain, searchResult);
            _entries.Add(entry);
        }
        public void AddRange(SearchResultEntryCollection searchResults)
        {
            foreach (SearchResultEntry searchResult in searchResults)
            {
                this.Add(searchResult);
            }
        }
        public void AddRange(IEnumerable<SearchResultEntry> searchResults)
        {
            foreach (SearchResultEntry searchResult in searchResults)
            {
                this.Add(searchResult);
            }
        }
        public void Clear()
        {
            _entries.Clear();
        }
        public int EnsureCapacity(int capacity)
        {
            return _entries.EnsureCapacity(capacity);
        }

        public void ReturnAll()
        {
            foreach (ResultEntry entry in _entries)
            {
                _pool.Return(entry);
            }

            this.Clear();
            this.Domain = string.Empty;
        }

        public IEnumerator<ResultEntry> GetEnumerator()
        {
            return new ArrayEnumerator<ResultEntry>(CollectionsMarshal.AsSpan(_entries));
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

