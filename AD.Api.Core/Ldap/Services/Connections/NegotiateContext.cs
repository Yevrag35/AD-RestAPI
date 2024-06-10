using AD.Api.Core.Security;
using AD.Api.Core.Settings;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Services.Connections
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class NegotiateContext : ConnectionContext
    {
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
    }
}

