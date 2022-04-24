using AD.Api.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace AD.Api.Services
{
    public interface IEncryptionService : IDisposable
    {
        bool CanPerform { get; }

        [return: NotNullIfNotNull("base64Encrypted")]
        byte[]? Decrypt(string? base64Encrypted);
    }

    public sealed class EncryptionService : IEncryptionService
    {
        private readonly X509Certificate2? _certificate;
        private bool _disposed;

        //private EncryptionSettings Settings { get; }
        //private ITextSettings TextSettings { get; }

        [MemberNotNull(nameof(_certificate))]
        public bool CanPerform { get; }

        public EncryptionService(IOptions<EncryptionSettings> options)
        {
            _certificate = FindCertificate(options.Value.SHA1Thumbprint);
            this.CanPerform = CertificateIsUsable(_certificate);
            //this.Settings = options.Value;
            //this.TextSettings = textSettings;
        }

        [return: NotNullIfNotNull("base64Encrypted")]
        public byte[]? Decrypt(string? base64Encrypted)
        {
            if (string.IsNullOrWhiteSpace(base64Encrypted))
                return null;

            var cms = new EnvelopedCms();
            cms.Decode(Convert.FromBase64String(base64Encrypted));

            cms.Decrypt();

            return cms.ContentInfo.Content;
        }

        #region BACKEND METHODS
        private static bool CertificateIsUsable(X509Certificate2? foundCertificate)
        {
            try
            {
                return foundCertificate is not null && foundCertificate.HasPrivateKey;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        private static X509Certificate2? FindCertificate(string? sha1Thumbprint)
        {
            if (string.IsNullOrWhiteSpace(sha1Thumbprint))
                return null;

            if (!TryFind(sha1Thumbprint, StoreLocation.CurrentUser, out X509Certificate2? userCert))
            {
                if (TryFind(sha1Thumbprint, StoreLocation.LocalMachine, out X509Certificate2? machineCert))
                    return machineCert;
            }

            return userCert;
        }

        private static bool TryFind(string sha1, StoreLocation storeLocation, [NotNullWhen(true)] out X509Certificate2? certificate)
        {
            certificate = null;
            using X509Store store = new(storeLocation);
            store.Open(OpenFlags.OpenExistingOnly);

            try
            {
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, sha1, false);
                foreach (X509Certificate2 cert in certs)
                {
                    certificate = cert;
                    break;      
                }
            }
            catch (CryptographicException)
            {
                return false;
            }
            finally
            {
                store.Close();
            }

            return certificate is not null;
        }

        #endregion

        #region IDISPOSABLE IMPLEMENTATION
        public void Dispose()
        {
            if (_disposed)
                return;

            _certificate?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
