using AD.Api.Core.Security.Accounts;
using AD.Api.Core.Settings.Credentials;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Text;

namespace AD.Api.Core.Security.Encryption
{
    public sealed class CertificateEncryptionService : IEncryptionService
    {
        public IEncryptionResult ReadCredentials(IConfigurationSection connectionSection)
        {
            EncryptionResult<CertificateEncryptedCredential> result = EncryptedCredential.FromSettings<CertificateEncryptedCredential>(connectionSection, this);



            if (result.HasCredential && result.Errors.Count <= 0)
            {
                result = this.Encrypt(result.Credential, result.Credential.GetEncoding());
            }

            return result;
        }

        public void SetCredentialPassword(EncryptedCredential credential, NetworkCredential networkCredential, ReadOnlySpan<char> encryptedPassword, Encoding encoding)
        {
            if (credential is not CertificateEncryptedCredential certCreds)
            {
                throw new ArgumentException("The certificate encryption service must work with the proper credentials.");
            }

            byte[] plainBytes = Decrypt(encryptedPassword);
            SecureString securePass = ReadInPlainBytes(plainBytes, encoding);

            networkCredential.SecurePassword = securePass;
            Array.Clear(plainBytes);
        }

        private static byte[] Decrypt(ReadOnlySpan<char> encryptedChars)
        {
            int minLength = ((encryptedChars.Length * 3) + 3) / 4;
            Span<byte> span = stackalloc byte[minLength];

            span = ReadOutEncrypted(encryptedChars, span);
            EnvelopedCms cms = new();
            cms.Decode(span);
            try
            {
                cms.Decrypt();
            }
            catch (Exception e)
            {
                throw new SecurityException("Failed to decrypt the encrypted string.", e);
            }

            return cms.ContentInfo.Content;
        }
        private EncryptionResult<CertificateEncryptedCredential> Encrypt(CertificateEncryptedCredential credential, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(credential.UserName) && !string.IsNullOrWhiteSpace(credential.EncryptedUserName))
            {
                byte[] rawUserName = Decrypt(credential.EncryptedUserName);
                credential.UserAccountName = AccountName.Parse(rawUserName, encoding);
                Array.Clear(rawUserName);
            }

            IReadOnlyList<ValidationResult> errors = ValidateAccountName(credential);
            if (errors.Count > 0)
            {
                return new EncryptionResult<CertificateEncryptedCredential>
                {
                    Credential = credential,
                    Errors = errors,
                };
            }

            if (!IsEncrypted(credential))
            {
                throw new SecurityException("Need encrypted credentials for now.");
            }

            if (errors.Count <= 0)
            {
                credential.StoreCredential(this);

                credential.Password = null;
                credential.UserName = null;
            }

            return new EncryptionResult<CertificateEncryptedCredential> { Credential = credential, Errors = errors };
        }

        private static bool IsEncrypted(CertificateEncryptedCredential credential)
        {
            return !string.IsNullOrWhiteSpace(credential.EncryptedUserName) && !string.IsNullOrWhiteSpace(credential.EncryptedPassword);
        }
        private static SecureString ReadInPlainBytes(Span<byte> plainBytes, Encoding encoding)
        {
            SecureString securePass = new();
            int length = encoding.GetMaxCharCount(plainBytes.Length);
            Span<char> chars = stackalloc char[length];
            int written = encoding.GetChars(plainBytes, chars);

            foreach (char c in chars.Slice(0, written))
            {
                securePass.AppendChar(c);
            }

            securePass.MakeReadOnly();
            return securePass;
        }
        private static Span<byte> ReadOutEncrypted(ReadOnlySpan<char> encryptedChars, Span<byte> bytes)
        {
            if (!Convert.TryFromBase64Chars(encryptedChars, bytes, out int written))
            {
                throw new FormatException("Invalid base64 sequence.");
            }

            return bytes.Slice(0, written);
        }
        private static IReadOnlyList<ValidationResult> ValidateAccountName(EncryptedCredential credential)
        {
            List<ValidationResult> errors = [];
            foreach (ValidationResult result in credential.UserAccountName.Validate(new ValidationContext(credential)))
            {
                if (ValidationResult.Success != result)
                {
                    errors.Add(result);
                }
            }

            return errors.Count > 0
                ? errors
                : Array.Empty<ValidationResult>();
        }
    }
}

