using AD.Api.Core.Ldap.Services.Connections;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;

namespace AD.Api.Core.Security
{
    public interface ILdapCredential : IDisposable
    {
        void SetCredential(LdapConnection connection);

        //[SupportedOSPlatform("WINDOWS")]
        //DirectoryContext CreateDirectoryContext(ConnectionContext context);
    }
}

