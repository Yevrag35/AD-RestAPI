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
        private SortedList<string, SearchDomain> _byFQDNs;
        private Dictionary<string, SearchDomain> _byDNs;
        private SearchDomain _registeredDefault;

        public static readonly StringComparer KeyComparer = StringComparer.CurrentCultureIgnoreCase;

        public SearchDomain this[int index]
        {
            get => _byFQDNs.Values[index];
        }
        public SearchDomain this[string key]
        {
            get => _byFQDNs[key];
        }

        public int Count => _byFQDNs.Count;
        IEnumerable<string> IReadOnlyDictionary<string, SearchDomain>.Keys => this.FQDNs;
        public ICollection<string> DNs => _byDNs.Keys;
        public IList<string> FQDNs => _byFQDNs.Keys;
        public ICollection<SearchDomain> Values => _byFQDNs.Values;
        IEnumerable<SearchDomain> IReadOnlyDictionary<string, SearchDomain>.Values => this.Values;

        public SearchDomains(IEnumerable<SearchDomain> domainsToAdd)
        {
            if (null == domainsToAdd)
                throw new ArgumentNullException(nameof(domainsToAdd));

            //_d = new OrderedDictionary(1, KeyComparer);
            _byFQDNs = new SortedList<string, SearchDomain>(1, KeyComparer);
            _byDNs = new Dictionary<string, SearchDomain>(1, KeyComparer);
            foreach (SearchDomain sd in domainsToAdd.OrderByDescending(x => x.IsDefault).ThenBy(x => x.FQDN))
            {
                if (sd.IsDefault && null == _registeredDefault)
                    _registeredDefault = sd;

                //_d.Add(sd.FQDN, sd);
                _byFQDNs.Add(sd.FQDN, sd);
                _byDNs.Add(sd.DistinguishedName, sd);
            }
        }
        public bool ContainsKey(string key)
        {
            return this.ContainsFQDN(key) || this.ContainsDN(key);
        }
        public bool ContainsFQDN(string fqdn)
        {
            return _byFQDNs.ContainsKey(fqdn);
        }
        public bool ContainsDN(string distinguishedName)
        {
            return _byDNs.ContainsKey(distinguishedName);
        }
        public string GetDefaultFQDN()
        {
            return this.GetDefaultDomain()?.FQDN;
        }
        public SearchDomain GetDefaultDomain()
        {
            if (this.Values.Count <= 0)
                return null;

            else if (null == _registeredDefault)
            {
                _registeredDefault = this[0];
            }

            return _registeredDefault;
        }
        public IEnumerator<KeyValuePair<string, SearchDomain>> GetEnumerator()
        {
            return _byFQDNs.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(string key, out SearchDomain domain)
        {
            domain = null;
            if (_byFQDNs.ContainsKey(key))
                domain = _byFQDNs[key];

            else if (_byDNs.ContainsKey(key))
                domain = _byDNs[key];

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
