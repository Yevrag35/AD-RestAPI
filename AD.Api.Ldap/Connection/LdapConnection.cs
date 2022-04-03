using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Path;
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
        private readonly LiveLdapObject _baseEntry;
        private readonly LiveLdapObject _rootDse;

        public LiveLdapObject RootDSE => _rootDse;
        public LiveLdapObject SearchBase => _baseEntry;

        public LdapConnection(ILdapConnectionOptions options)
        {
            _options = options;
            var path = new PathValue(options.Protocol)
            {
                DistinguishedName = options.DistinguishedName,
                Host = options.Host,
                UseSsl = options.UseSSL
            };

            var rootDse = new PathValue(options.Protocol)
            {
                DistinguishedName = "RootDSE",
                Host = options.Host,
                UseSsl = options.UseSSL
            };

            //_baseEntry = new LiveLdapObject(CreateEntry(path, options), path);
            //_rootDse = new LiveLdapObject(CreateEntry(rootDse, options), rootDse);
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

            _baseEntry.Dispose();
            _rootDse.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
