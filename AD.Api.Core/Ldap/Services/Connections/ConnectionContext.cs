using AD.Api.Core.Security;
using AD.Api.Core.Settings;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public abstract class ConnectionContext
    {
        private readonly RegisteredDomain _domain;
        private readonly LdapDirectoryIdentifier _identifier;

        public string Name { get; }

        protected ConnectionContext(RegisteredDomain domain, string connectionName)
        {
            ArgumentNullException.ThrowIfNull(domain);
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
            _domain = domain;
            this.Name = connectionName;
            _identifier = this.CreateIdentifier(domain);
        }

        public LdapConnection CreateConnection()
        {
            return this.CreateConnection(_domain, _identifier);
        }

        protected abstract LdapConnection CreateConnection(RegisteredDomain domain, LdapDirectoryIdentifier identifier);

        protected virtual LdapDirectoryIdentifier CreateIdentifier(RegisteredDomain domain)
        {
            if (domain.DomainControllers.Length <= 0)
            {
                if (!domain.PortNumber.HasValue)
                {
                    domain.PortNumber = 389;
                }

                return new LdapDirectoryIdentifier(domain.DomainName, domain.PortNumber.Value, false, false);
            }
            else if (!domain.UseSSL.HasValue && !domain.PortNumber.HasValue)
            {
                return new LdapDirectoryIdentifier(domain.DomainControllers, false, false);
            }
            else if (domain.UseSSL.HasValue)
            {
                return new LdapDirectoryIdentifier(domain.DomainControllers, domain.UseSSL.Value ? 636 : 389, false, false);
            }
            else
            {
                return new LdapDirectoryIdentifier(domain.DomainControllers, domain.PortNumber ?? 389, false, false);
            }
        }
    }
}

