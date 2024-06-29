using AD.Api.Core.Schema;
using NLog;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap
{
    public abstract class ConnectionContext : IEquatable<ConnectionContext>, IServiceProvider
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly RegisteredDomain _domain;
        private readonly LdapIdentifierDictionary _identifiers;
        private readonly IServiceProvider _provider;
        public string DefaultNamingContext => _domain.DefaultNamingContext;
        public string DomainName => _domain.DomainName;
        public bool IsDefault => _domain.IsDefault;
        public bool IsForestRoot => _domain.IsForestRoot;
        public string Name { get; }

        protected ConnectionContext(RegisteredDomain domain, string connectionName, IServiceProvider provider)
        {
            ArgumentNullException.ThrowIfNull(domain);
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
            _domain = domain;
            this.Name = connectionName;
            var identifier = this.CreateIdentifier(domain);
            _identifiers = new(domain.DomainName, identifier);

            _provider = provider;
        }

        public LdapConnection CreateConnection()
        {
            return this.CreateConnection(_domain, _identifiers.Default);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainController"></param>
        /// <returns></returns>
        /// <inheritdoc cref="LdapConnection.Bind()" path="/exception"/>
        public LdapConnection CreateConnection(string? domainController)
        {
            if (string.IsNullOrWhiteSpace(domainController) || _domain.DomainName.Equals(domainController, StringComparison.OrdinalIgnoreCase))
            {
                return this.CreateConnection();
            }

            LdapDirectoryIdentifier identifier = _identifiers.GetOrAdd(domainController);
            LdapConnection connection = this.CreateConnection(_domain, identifier);
            try
            {
                connection.Bind();
                return connection;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                _identifiers.TryRemove(domainController);
                try
                {
                    connection.Dispose();
                }
                catch (Exception ie)
                {
                    _logger.Warn(ie);
                }

                throw;
            }
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

        public object? GetService(Type serviceType)
        {
            return _provider.GetService(serviceType);
        }

        [SupportedOSPlatform("WINDOWS")]
        internal void SetSchemaBuilder(SchemaDictionaryBuilder builder, Action<RegisteredDomain, DirectoryContext, SchemaDictionaryBuilder> action)
        {
            if (!this.TryGetDirectoryContext(DirectoryContextType.Forest, out DirectoryContext? context))
            {
                return;
            }

            action(_domain, context, builder);
        }

        [SupportedOSPlatform("WINDOWS")]
        public abstract bool TryGetDirectoryContext(DirectoryContextType contextType, [NotNullWhen(true)] out DirectoryContext? directoryContext);
    }
}

