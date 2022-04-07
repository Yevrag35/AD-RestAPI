using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;
using AD.Api.Extensions;

namespace AD.Api.Domains
{
    public class SearchDomains : IReadOnlyDictionary<string, SearchDomain>
    {
        //private OrderedDictionary _d;
        private readonly SortedList<string, SearchDomain> _byFQDNs;
        private readonly Dictionary<string, SearchDomain> _byDNs;
        private SearchDomain? _registeredDefault;

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

        public SearchDomains(IEnumerable<SearchDomain>? domainsToAdd)
        {
            if (domainsToAdd is null)
                throw new ArgumentNullException(nameof(domainsToAdd));

            //_d = new OrderedDictionary(1, KeyComparer);
            _byFQDNs = new SortedList<string, SearchDomain>(1, KeyComparer);
            _byDNs = new Dictionary<string, SearchDomain>(1, KeyComparer);

            foreach (SearchDomain domain in domainsToAdd.OrderByDescending(x => x.IsDefault).ThenBy(x => x.FQDN))
            {
                if (domain.IsDefault && _registeredDefault is null)
                    _registeredDefault = domain;

                _byFQDNs.Add(domain.FQDN, domain);

                if (!TryValidateDomain(domain, out string? domainNamingContext))
                {
                    throw new InvalidOperationException($"Unable to find the default naming context for domain '{domain.FQDN}.");
                }

                domain.DistinguishedName = domainNamingContext;

                _byDNs.Add(domainNamingContext, domain);
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
        public string? GetDefaultFQDN()
        {
            return this.GetDefaultDomain()?.FQDN;
        }
        public SearchDomain? GetDefaultDomain()
        {
            if (this.Values.Count <= 0)
                return null;

            else if (_registeredDefault is null)
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

        public bool TryGetValue(string? key, [NotNullWhen(true)] out SearchDomain? domain)
        {
            domain = null;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (_byFQDNs.ContainsKey(key))
                domain = _byFQDNs[key];

            else if (_byDNs.ContainsKey(key))
                domain = _byDNs[key];

            return null != domain;
        }

        private static bool TryValidateDomain(SearchDomain domain, [NotNullWhen(true)] out string? distinguishedName)
        {
            distinguishedName = domain.DistinguishedName;
            if (!string.IsNullOrWhiteSpace(distinguishedName))
                return true;

            using DirectoryEntry tempEntry = new($"LDAP://{domain.FQDN}/RootDSE");
            try
            {
                tempEntry.RefreshCache();
                if (tempEntry.Properties.TryGetFirstValue("defaultNamingContext", out string? defNamingContext))
                {
                    distinguishedName = defNamingContext;
                }
            }
            finally
            {
                tempEntry.Dispose();
            }

            return !string.IsNullOrWhiteSpace(distinguishedName);
        }
    }

    public class SearchDomain
    {
        public string? DistinguishedName { get; set; }
        public bool IsDefault { get; set; }
        public string FQDN { get; set; } = string.Empty;
        public string? StaticDomainController { get; set; }
        public bool UseGlobalCatalog { get; set; }
        public bool UseSSL { get; set; }
    }
}
