using System.Net;
using System.Text;

namespace AD.Api.Core.Security.Encryption
{
    public interface IEncryptionService
    {
        EncryptionResult Encrypt(EncryptedCredential credential, Encoding encoding);
        //bool IsEncrypted(EncryptedCredential credential);
        EncryptionResult ReadCredentials(IConfigurationSection connectionSection);
        void SetCredentialPassword(NetworkCredential credential, ReadOnlySpan<char> encryptedPassword, Encoding encoding);
    }
}

