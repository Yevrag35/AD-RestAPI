using AD.Api.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Core.Settings.Credentials
{
    public sealed class CertificateEncryptedCredential : EncryptedCredential
    {
        public required string SHA1Thumbprint { get; init; } = string.Empty;
    }
}

