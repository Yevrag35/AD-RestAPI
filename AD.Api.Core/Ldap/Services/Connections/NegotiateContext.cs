using AD.Api.Core.Security;
using AD.Api.Core.Settings;
using AD.Api.Startup.Exceptions;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Services.Connections
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class NegotiateContext : ConnectionContext
    {
        public NegotiateContext(Forest forest, string connectionName)
            : this(FromForest(forest, connectionName), connectionName)
        {
        }
        public NegotiateContext(RegisteredDomain domain, string connectionName) : base(domain, connectionName)
        {
        }

        protected override LdapConnection CreateConnection(RegisteredDomain domain, LdapDirectoryIdentifier identifier)
        {
            return new LdapConnection(identifier)
            {
                AutoBind = true,
                AuthType = AuthType.Negotiate,
            };
        }

        private static RegisteredDomain FromForest(Forest forest, string connectionName)
        {
            using DirectoryEntry rootDse = new($"LDAP://{forest.SchemaRoleOwner.Name}/RootDSE");
            string namingContext = GetValue<string>(rootDse, "defaultNamingContext");
            return new RegisteredDomain()
            {
                DefaultNamingContext = namingContext,
                IsForestRoot = true,
                DomainName = forest.Name,
                Name = connectionName,
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
    }
}

