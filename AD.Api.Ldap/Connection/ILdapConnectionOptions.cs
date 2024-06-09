using Microsoft.Win32.SafeHandles;
using System.DirectoryServices;
using System.Net;

namespace AD.Api.Ldap.Connection
{
    public interface ILdapConnectionOptions
    {
        AuthenticationTypes? AuthenticationTypes { get; }
        string? DistinguishedName { get; }
        bool DontDisposeToken { get; }
        bool IsForest { get; }
        string? Host { get; }
        Protocol Protocol { get; }
        SafeAccessTokenHandle? Token { get; }
        bool UseSchemaCache { get; }
        bool UseSSL { get; }

        NetworkCredential? GetCredential();
    }
}
