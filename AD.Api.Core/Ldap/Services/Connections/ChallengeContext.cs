using AD.Api.Core.Security;
using AD.Api.Core.Settings;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public sealed class ChallengeContext : ConnectionContext
    {
        private readonly ILdapCredential _credential;

        public ChallengeContext(RegisteredDomain domain, string connectionName, ILdapCredential credential)
            : base(domain, connectionName)
        {
            _credential = credential;
        }

        protected override LdapConnection CreateConnection(RegisteredDomain domain, LdapDirectoryIdentifier identifier)
        {
            LdapConnection con = new(identifier)
            {
                AutoBind = true,
                AuthType = AuthType.Ntlm,
            };

            _credential.SetCredential(con);
            return con;
        }
    }
}

