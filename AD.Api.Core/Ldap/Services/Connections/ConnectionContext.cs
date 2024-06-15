using AD.Api.Core.Schema;
using AD.Api.Core.Security;
using AD.Api.Core.Settings;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public abstract class ConnectionContext : IEquatable<ConnectionContext>
    {
        private readonly RegisteredDomain _domain;
        private readonly LdapDirectoryIdentifier _identifier;
        public string DefaultNamingContext => _domain.DefaultNamingContext;
        public string DomainName => _domain.DomainName;
        public bool IsForestRoot => _domain.IsForestRoot;
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

        public virtual bool Equals([NotNullWhen(true)] ConnectionContext? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            else if (other is null)
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(this.DomainName, other.DomainName)
                && this.IsForestRoot == other.IsForestRoot;
        }
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            else if (obj is ConnectionContext other)
            {
                return this.Equals(other);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            int code = StringComparer.OrdinalIgnoreCase.GetHashCode(this.DomainName);
            return HashCode.Combine(code, this.IsForestRoot);
        }

        [SupportedOSPlatform("WINDOWS")]
        public abstract bool TryGetDirectoryContext(DirectoryContextType contextType, [NotNullWhen(true)] out DirectoryContext? directoryContext);

        [SupportedOSPlatform("WINDOWS")]
        internal void SetSchemaBuilder(SchemaDictionaryBuilder builder, Action<RegisteredDomain, DirectoryContext, SchemaDictionaryBuilder> action)
        {
            if (!this.TryGetDirectoryContext(DirectoryContextType.Forest, out DirectoryContext? context))
            {
                return;
            }

            action(_domain, context, builder);
        }
    }
}

