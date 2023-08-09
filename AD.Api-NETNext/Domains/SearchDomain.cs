using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;
using AD.Api.Exceptions;
using AD.Api.Extensions;

namespace AD.Api.Domains
{
    public class SearchDomains : IReadOnlyDictionary<string, SearchDomain>
    {
        private readonly SortedList<string, SearchDomain> _fullyQualifiedDictionary;
        private readonly Dictionary<string, SearchDomain> _distinguishedNameDictionary;
        private SearchDomain? _registeredDefault;

        public static readonly StringComparer KeyComparer = StringComparer.CurrentCultureIgnoreCase;

        public SearchDomain this[int index]
        {
            get => _fullyQualifiedDictionary.Values[index];
        }
        public SearchDomain this[string key]
        {
            get => _fullyQualifiedDictionary[key];
        }

        public int Count => _fullyQualifiedDictionary.Count;
        IEnumerable<string> IReadOnlyDictionary<string, SearchDomain>.Keys => this.FQDNs;
        public ICollection<string> DNs => _distinguishedNameDictionary.Keys;
        public IList<string> FQDNs => _fullyQualifiedDictionary.Keys;
        public ICollection<SearchDomain> Values => _fullyQualifiedDictionary.Values;
        IEnumerable<SearchDomain> IReadOnlyDictionary<string, SearchDomain>.Values => this.Values;

        public SearchDomains(IEnumerable<SearchDomain>? domainsToAdd)
        {
            if (domainsToAdd is null)
            {
                throw new ArgumentNullException(nameof(domainsToAdd));
            }

            _fullyQualifiedDictionary = new SortedList<string, SearchDomain>(1, KeyComparer);
            _distinguishedNameDictionary = new Dictionary<string, SearchDomain>(1, KeyComparer);

            foreach (SearchDomain domain in domainsToAdd.OrderByDescending(x => x.IsDefault).ThenBy(x => x.FQDN))
            {
                if (domain.IsDefault && _registeredDefault is null)
                {
                    _registeredDefault = domain;
                }

                _fullyQualifiedDictionary.Add(domain.FQDN, domain);

                if (!TryValidateDomain(domain, out string? domainNamingContext))
                {
                    throw new NoNamingContextException(domain.FQDN);
                }

                domain.DistinguishedName = domainNamingContext;

                _distinguishedNameDictionary.Add(domainNamingContext, domain);
            }
        }

        public bool ContainsKey(string key)
        {
            return this.ContainsFQDN(key) || this.ContainsDN(key);
        }
        public bool ContainsFQDN(string fqdn)
        {
            return _fullyQualifiedDictionary.ContainsKey(fqdn);
        }
        public bool ContainsDN(string distinguishedName)
        {
            return _distinguishedNameDictionary.ContainsKey(distinguishedName);
        }
        public string? GetDefaultFQDN()
        {
            return this.GetDefaultDomain()?.FQDN;
        }
        public SearchDomain? GetDefaultDomain()
        {
            if (this.Values.Count <= 0)
            {
                return null;
            }
            else if (_registeredDefault is null)
            {
                _registeredDefault = this[0];
            }

            return _registeredDefault;
        }
        public IEnumerator<KeyValuePair<string, SearchDomain>> GetEnumerator()
        {
            return _fullyQualifiedDictionary.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(string? key, [NotNullWhen(true)] out SearchDomain? domain)
        {
            domain = null;
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (_fullyQualifiedDictionary.ContainsKey(key))
            {
                domain = _fullyQualifiedDictionary[key];
            }
            else if (_distinguishedNameDictionary.ContainsKey(key))
            {
                domain = _distinguishedNameDictionary[key];
            }

            return null != domain;
        }

        private static bool TryValidateDomain(SearchDomain domain, [NotNullWhen(true)] out string? distinguishedName)
        {
            distinguishedName = domain.DistinguishedName;
            if (!string.IsNullOrWhiteSpace(distinguishedName))
            {
                return true;
            }

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
    #region DEPENDENCY INJECTION
    public static class SearchDomainDependencyInjectionExtensions
    {
        public static IServiceCollection AddSearchDomains(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            IEnumerable<SearchDomain> domains = ParseDomainsFromConfig(configurationSection.GetChildren());
            var collection = new SearchDomains(domains);

            return services.AddSingleton(collection);
        }

        private static IEnumerable<SearchDomain> ParseDomainsFromConfig(IEnumerable<IConfigurationSection> sections)
        {
            foreach (IConfigurationSection section in sections)
            {
                SearchDomain dom = section.Get<SearchDomain>();
                dom.FQDN = section.Key;
                yield return dom;
            }
        }
    }

    #endregion
    public class SearchDomain
    {
        private bool _useSchemaCache;

        public string? DistinguishedName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsForestRoot { get; set; }
        public string FQDN { get; set; } = string.Empty;
        public string? StaticDomainController { get; set; }
        public bool UseGlobalCatalog { get; set; }
        public bool UseSchemaCache
        {
            get => _useSchemaCache && this.IsForestRoot;
            set => _useSchemaCache = value;
        }
        public bool UseSSL { get; set; }
    }
}
