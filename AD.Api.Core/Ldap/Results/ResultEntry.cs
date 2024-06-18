using AD.Api.Core.Serialization;
using Microsoft.Extensions.ObjectPool;
using System.Collections;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Results
{
    public sealed class ResultEntry : IReadOnlyCollection<KeyValuePair<string, object>>, IResettable
    {
        private readonly SortedDictionary<string, object> _attributes;
        private readonly IAttributeConverter _converter;

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

        public bool TryReset()
        {
            this.LeaseId = Guid.Empty;
            _attributes.Clear();
            return _attributes.Count == 0;
        }
    }
}

