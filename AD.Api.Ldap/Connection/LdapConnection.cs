using AD.Api.Extensions;
using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Path;
using AD.Api.Ldap.Search;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;

using Strings = AD.Api.Ldap.Properties.Resources;

namespace AD.Api.Ldap
{
    public sealed class LdapConnection : IDisposable
    {
        private bool _disposed;
        private readonly ILdapConnectionOptions _options;

        public bool IsForestRoot => _options.IsForest;
        public PathValue RootDSE { get; }
        public PathValue SearchBase { get; }

        public LdapConnection(ILdapConnectionOptions options)
        {
            _options = options;
            this.RootDSE = new PathValue(options.Protocol)
            {
                DistinguishedName = "RootDSE",
                Host = options.Host,
                UseSsl = options.UseSSL
            };

            this.SearchBase = new PathValue(options.Protocol)
            {
                DistinguishedName = options.DistinguishedName,
                Host = options.Host,
                UseSsl = options.UseSSL
            };
        }

        public DirectoryEntry GetDirectoryEntry(string dn)
        {
            if (string.IsNullOrWhiteSpace(dn))
                throw new ArgumentNullException(nameof(dn));

            return this.GetDirectoryEntry(new PathValue(_options.Protocol)
            {
                DistinguishedName = dn,
                Host = _options.Host,
                UseSsl = _options.UseSSL
            });
        }
        public DirectoryEntry GetDirectoryEntry(PathValue path)
        {
            return CreateEntry(path, _options);
        }

        public DirectoryEntry? GetDirectoryEntry(IPathed? pathedObject)
        {
            return pathedObject is not null && pathedObject.Path is not null
                ? this.GetDirectoryEntry(pathedObject.Path)
                : null;
        }

        public DirectoryEntry GetRootDSE()
        {
            return this.GetDirectoryEntry(this.RootDSE);
        }

        public DirectoryEntry GetSearchBase()
        {
            return this.GetDirectoryEntry(this.SearchBase);
        }

        public Searcher CreateSearcher(PathValue searchBase, IFilterStatement? filter = null)
        {
            return new Searcher(this.GetDirectoryEntry(searchBase))
            {
                Filter = filter
            };
        }

        public Searcher CreateSearcher(IFilterStatement filter)
        {
            return new Searcher(this)
            {
                Filter = filter
            };
        }

        public Searcher CreateSearcher(ISearchOptions? options = null)
        {
            return new Searcher(this, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public DirectoryContext GetForestContext()
        {
            if (!this.IsForestRoot)
                throw new InvalidOperationException($"Cannot get AD forest context when the connection was not specified as the Forest Root.");

            string host;
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                using DirectoryEntry rootDse = this.GetRootDSE();
                host = GetForestName(rootDse);
            }
            else
                host = _options.Host;

            NetworkCredential? creds = _options.GetCredential();
            return creds is null
                ? new DirectoryContext(DirectoryContextType.Forest, host)
                : new DirectoryContext(DirectoryContextType.Forest, host, creds.UserName, creds.Password);
        }
        
        public PathValue GetWellKnownPath(WellKnownObjectValue value)
        {
            using DirectoryEntry rootDse = this.GetRootDSE();

            if (!rootDse.Properties.TryGetFirstValue(Strings.DefaultNamingContext, out string? namingContext))
            {
                rootDse.Dispose();
                throw new InvalidOperationException();  // Figure out what to do later.
            }

            WellKnownObject wko = WellKnownObject.Create(value, namingContext);

            return new PathValue(_options.Protocol)
            {
                DistinguishedName = wko,
                Host = _options.Host,
                UseSsl = _options.UseSSL
            };
        }

        private static DirectoryEntry CreateEntry(PathValue path, ILdapConnectionOptions options)
        {
            NetworkCredential? creds = options.GetCredential();
            if (creds is null)
                return new DirectoryEntry(path.GetValue());

            else
            {
                return !options.AuthenticationTypes.HasValue
                    ? new DirectoryEntry(path.GetValue(), creds.UserName, creds.Password)
                    : new DirectoryEntry(path.GetValue(), creds.UserName, creds.Password, options.AuthenticationTypes.Value);
            }
        }

        private static string GetForestName(DirectoryEntry rootDse)
        {
            if (!rootDse.Properties.TryGetFirstValue("dnsHostName", out string? hostName))
                throw new InvalidOperationException($"Unable to get the connection name from RootDSE");

            return hostName;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            //_baseEntry.Dispose();
            //_rootDse.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
