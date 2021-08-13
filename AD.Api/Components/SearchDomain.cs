using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AD.Api.Extensions;
using AD.Api.Models;

namespace AD.Api.Components
{
    [SupportedOSPlatform("windows")]
    public class SearchDomains : IReadOnlyDictionary<string, SearchDomain>
    {
        private OrderedDictionary _d;

        public static readonly StringComparer KeyComparer = StringComparer.CurrentCultureIgnoreCase;

        public SearchDomain this[string key]
        {
            get => _d[key] as SearchDomain;
        }

        public int Count => _d.Count;
        public IEnumerable<string> Keys => _d.Keys.Cast<string>();
        public IEnumerable<SearchDomain> Values => _d.Values.Cast<SearchDomain>();

        public SearchDomains(IEnumerable<SearchDomain> domainsToAdd)
        {

            if (null == domainsToAdd)
                throw new ArgumentNullException(nameof(domainsToAdd));

            _d = new OrderedDictionary(KeyComparer);
            foreach (SearchDomain sd in domainsToAdd)
            {
                _d.Add(sd.FQDN, sd);
            }
        }
        public bool ContainsKey(string key)
        {
            return _d.Contains(key);
        }
        public string GetDefault()
        {
            if (_d.Count <= 0)
                return null;

            string def = this.Values.FirstOrDefault(x => x.IsDefault)?.FQDN;
            if (string.IsNullOrWhiteSpace(def))
            {
                def = ((SearchDomain)_d[0]).FQDN;
            }

            return def;
        }
        public IEnumerator<KeyValuePair<string, SearchDomain>> GetEnumerator()
        {
            foreach (string key in _d.Keys)
            {
                SearchDomain sd = _d[key] as SearchDomain;
                yield return new KeyValuePair<string, SearchDomain>(key, sd);
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(string key, out SearchDomain domain)
        {
            domain = null;
            if (_d.Contains(key))
            {
                domain = _d[key] as SearchDomain;
            }

            return null != domain;
        }
    }

    [SupportedOSPlatform("windows")]
    public class SearchDomain : IDirObject
    {
        public string DistinguishedName { get; set; }
        public bool IsDefault { get; set; }
        public string FQDN { get; set; }
        public string StaticDomainController { get; set; }

        public DirectoryEntry GetDirectoryEntry(string domainController = null)
        {
            if (string.IsNullOrWhiteSpace(domainController))
                domainController = this.StaticDomainController;

            return new DirectoryEntry(this.DistinguishedName.ToLdapPath(domainController));
        }
    }
}
