using Microsoft.Extensions.ObjectPool;
using System.Collections;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Results
{
    public sealed class ResultEntryCollection : IEnumerable<ResultEntry>, IResettable, ISearchResultEntry
    {
        private readonly List<ResultEntry> _entries;
        private readonly ResultEntryPool _pool;

        public int Count => _entries.Count;
        public string Domain { get; set; }
        public Guid LeaseId { get; set; }

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
        private void ReturnAll()
        {
            foreach (ResultEntry entry in _entries)
            {
                _pool.Return(entry);
            }

            this.Clear();
        }
        public bool TryReset()
        {
            this.Domain = string.Empty;
            this.LeaseId = Guid.Empty;
            this.ReturnAll();
            return this.Count == 0;
        }

        public IEnumerator<ResultEntry> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryApplyResponse(string? domain, [NotNullWhen(true)] SearchResponse? response)
        {
            if (response is null)
            {
                return false;
            }

            this.Domain = domain ?? string.Empty;
            if (response.Entries.Count > 0)
            {
                this.AddRange(response.Entries);
            }

            return true;
        }
    }
}

