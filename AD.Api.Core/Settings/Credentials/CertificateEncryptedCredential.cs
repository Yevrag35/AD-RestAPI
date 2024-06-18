using AD.Api.Core.Security;

namespace AD.Api.Core.Settings.Credentials
{
    public sealed class CertificateEncryptedCredential : EncryptedCredential
    {
        public required string SHA1Thumbprint { get; init; } = string.Empty;
    }
}

