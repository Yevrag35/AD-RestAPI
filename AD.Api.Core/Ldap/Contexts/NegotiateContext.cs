using AD.Api.Startup.Exceptions;
using System.Collections.Concurrent;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class NegotiateContext : ConnectionContext
    {
        private readonly ConcurrentDictionary<DirectoryContextType, DirectoryContext> _dirContexts = null!;

        public NegotiateContext(Forest forest, bool isDefault, string connectionName, IServiceProvider services)
            : this(FromForest(forest, isDefault, connectionName), connectionName, services)
        {
        }
        public NegotiateContext(RegisteredDomain domain, string connectionName, IServiceProvider provider)
            : base(domain, connectionName, provider)
        {
            _dirContexts = new(Environment.ProcessorCount, 1);
            bool added = _dirContexts.TryAdd(DirectoryContextType.Domain, new(DirectoryContextType.Domain, domain.DomainName));
            Debug.Assert(added);

            if (domain.IsForestRoot)
            {
                added = _dirContexts.TryAdd(DirectoryContextType.Forest, new(DirectoryContextType.Forest, domain.DomainName));
                Debug.Assert(added);
            }
        }

        protected override LdapConnection CreateConnection(RegisteredDomain domain, LdapDirectoryIdentifier identifier)
        {
            return new LdapConnection(identifier)
            {
                AutoBind = true,
                AuthType = AuthType.Negotiate,
            };
        }

        private static RegisteredDomain FromForest(Forest forest, bool isDefault, string connectionName)
        {
            using DirectoryEntry rootDse = new($"LDAP://{forest.SchemaRoleOwner.Name}/RootDSE");
            string namingContext = GetValue<string>(rootDse, "defaultNamingContext");
            return new RegisteredDomain()
            {
                DefaultNamingContext = namingContext,
                IsForestRoot = true,
                DomainName = forest.Name,
                Name = connectionName,
                IsDefault = isDefault,
            };
        }

        private static T GetValue<T>(DirectoryEntry entry, string propertyName)
        {
            try
            {
                return (T)entry.Properties[propertyName].Value!;
            }
            catch (Exception e)
            {
                throw new AdApiStartupException(typeof(NegotiateContext), e);
            }
        }

        public override bool TryGetDirectoryContext(DirectoryContextType contextType, [NotNullWhen(true)] out DirectoryContext? directoryContext)
        {
            if (!_dirContexts.TryGetValue(contextType, out directoryContext))
            {
                directoryContext = new(contextType, this.DomainName);
                _ = _dirContexts.TryAdd(contextType, directoryContext);
            }

            return true;
        }
    }
}

