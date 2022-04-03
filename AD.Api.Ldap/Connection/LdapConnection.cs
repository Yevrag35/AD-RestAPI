using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Path;
using AD.Api.Ldap.Search;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;

namespace AD.Api.Ldap
{
    public sealed class LdapConnection : IDisposable
    {
        private bool _disposed;
        private readonly ILdapConnectionOptions _options;

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
