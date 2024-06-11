using System.Net;
using System.Text;

namespace AD.Api.Core.Security.Encryption
{
    public interface IEncryptionService
    {
        IEncryptionResult ReadCredentials(IConfigurationSection connectionSection);
        void SetCredentialPassword(EncryptedCredential credential, NetworkCredential networkCredential, ReadOnlySpan<char> encryptedPassword, Encoding encoding);
    }
}

