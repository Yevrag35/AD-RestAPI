using AD.Api.Core.Serialization;
using Microsoft.Extensions.ObjectPool;
using System.Collections;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Results
{
    [DebuggerDisplay(@"Count = {Count} ({LeaseId})")]
    public sealed class ResultEntry : IReadOnlyCollection<KeyValuePair<string, object>>, IResettable, ISearchResultEntry
    {
        private readonly SortedDictionary<string, object> _attributes;
        private readonly IAttributeConverter _converter;

        public object this[string key] => _attributes[key];

        public int Count => _attributes.Count;
        public Guid LeaseId { get; set; }

        public ResultEntry(IAttributeConverter converter)
        {
            _attributes = new(StringComparer.OrdinalIgnoreCase);
            _converter = converter;
        }

        public void AddResult(string domain, SearchResultEntry entry)
        {
            _converter.ConvertEntry(domain, entry, _attributes);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public bool TryApplyResponse(string? domain, [NotNullWhen(true)] SearchResponse? response)
        {
            if (response is null || response.Entries.Count != 1)
            {
                return false;
            }

            this.AddResult(domain ?? string.Empty, response.Entries[0]);
            return true;
        }

        public bool TryReset()
        {
            this.LeaseId = Guid.Empty;
            _attributes.Clear();
            return _attributes.Count == 0;
        }
    }
}

