using Linq2Ldap;
using Linq2Ldap.Core.Attributes;
using Linq2Ldap.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AD.Api.Components;
using AD.Api.Extensions;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Services
{
    [SupportedOSPlatform("windows")]
    public abstract class SearchServiceBase<T> : IDisposable
        where T : IEntry, new()
    {
        private bool _disposed;
        private Searcher<T> _searcher;

        protected SearchDomains Domains { get; }
        protected Searcher<T> Searcher
        {
            get => _searcher;
            set
            {
                _searcher?.Dispose();
                _searcher = value;
            }
        }

        public SearchServiceBase(SearchDomains domains)
        {
            this.Domains = domains;
            _searcher = new Searcher<T>(this.Domains);
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                this.Searcher?.Dispose();
                _disposed = true;
            }
        }

        protected virtual void ValidateDomain(string domain)
        {
            //if (!this.Searcher.SwitchDirectories(domain))
            //    throw new ArgumentException(Strings.Error_InvalidDomainName.Format(domain));
        }
    }
}
