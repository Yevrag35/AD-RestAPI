using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Security
{
    public interface ILdapCredential : IDisposable
    {
        void SetCredential(LdapConnection connection);
    }
}

