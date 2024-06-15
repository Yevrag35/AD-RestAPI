using AD.Api.Core.Security;
using AD.Api.Core.Settings;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public sealed class ChallengeContext : ConnectionContext
    {
        private readonly ILdapCredential _credential;
        [SupportedOSPlatform("WINDOWS")]
        private readonly Dictionary<DirectoryContextType, DirectoryContext> _dirContexts = null!;

        public ChallengeContext(RegisteredDomain domain, string connectionName, ILdapCredential credential)
            : base(domain, connectionName)
        {
            _credential = credential;
            if (OperatingSystem.IsWindows())
            {
                _dirContexts = [];
            }
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

        [SupportedOSPlatform("WINDOWS")]
        public override bool TryGetDirectoryContext(DirectoryContextType contextType, [NotNullWhen(true)] out DirectoryContext? directoryContext)
        {
            if (contextType == DirectoryContextType.Forest && !this.IsForestRoot)
            {
                Debug.Fail($"'{this.DomainName}' is not a forest root.");
                directoryContext = null;
                return false;
            }

            if (!_dirContexts.TryGetValue(contextType, out directoryContext))
            {
                try
                {
                    directoryContext = new(contextType, this.DomainName);
                }
                catch (Exception e)
                {
                    Debug.Fail(e.Message);
                    return false;
                }
            }

            return true;
        }
    }
}

