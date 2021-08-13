using Linq2Ldap;
using Linq2Ldap.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.Versioning;

namespace AD.Api.Components
{
    [SupportedOSPlatform("windows")]
    public class Searcher<T> : LinqDirectorySearcher<T> where T : IEntry, new()
    {
        private bool _disposed;
        private ReadOnlyDictionary<string, DirectoryEntry> _domainEntries;

        public string Current { get; private set; }
        public string DefaultDomain { get; }

        public Searcher(SearchDomains domains)
            : base()
        {
            //this.AddAttributes();
            this.DefaultDomain = domains.GetDefault();

            _domainEntries = new ReadOnlyDictionary<string, DirectoryEntry>(
                domains.ToDictionary(
                    x => x.Key,
                    e => e.Value.GetDirectoryEntry(),
                    StringComparer.CurrentCultureIgnoreCase
                )
            );

            this.SearchRoot = _domainEntries[this.DefaultDomain];
        }
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                foreach (var kvp in _domainEntries)
                {
                    kvp.Value.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
