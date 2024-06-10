using AD.Api.Core.Security;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace AD.Api.Core.Settings.Credentials
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class DpApiEncryptedCredential : EncryptedCredential
    {
        public DataProtectionScope DpapiScope { get; init; } = DataProtectionScope.CurrentUser;
    }
}
