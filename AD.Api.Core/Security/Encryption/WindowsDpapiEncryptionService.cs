using AD.Api.Core.Security.Accounts;
using AD.Api.Core.Settings.Credentials;
using AD.Api.Strings;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace AD.Api.Core.Security.Encryption
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class WindowsDpapiEncryptionService : IEncryptionService
    {
        public WindowsDpapiEncryptionService()
        {
        }

        public byte[] Decrypt(ReadOnlySpan<char> encryptedBase64Chars)
        {
            return this.Decrypt(encryptedBase64Chars, DataProtectionScope.CurrentUser);
        }
        internal byte[] Decrypt(ReadOnlySpan<char> encryptedBase64Chars, DataProtectionScope scope)
        {
            int minLength = Base64.IsValid(encryptedBase64Chars)
                ? Base64Extensions.GetByteLength(encryptedBase64Chars)
                : Encoding.UTF8.GetMaxByteCount(encryptedBase64Chars.Length);

            Span<byte> span = stackalloc byte[minLength];

            byte[] encryptedBytes = ReadOutEncrypted(encryptedBase64Chars, span).ToArray();
            byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, scope);

            Array.Clear(encryptedBytes);
            return plainBytes;
        }

        public IEncryptionResult ReadCredentials(IConfigurationSection connectionSection)
        {
            EncryptionResult<DpApiEncryptedCredential> result = EncryptedCredential.FromSettings<DpApiEncryptedCredential>(connectionSection, this);
            if (result.HasCredential && result.Errors.Count <= 0)
            {
                result = this.Encrypt(result.Credential, result.Credential.GetEncoding());
            }

            return result;
        }
        private static Span<byte> ReadOutEncrypted(ReadOnlySpan<char> encryptedChars, Span<byte> bytes)
        {
            if (!Convert.TryFromBase64Chars(encryptedChars, bytes, out int written)
                &&
                !Encoding.UTF8.TryGetBytes(encryptedChars, bytes, out written))
            {
                throw new FormatException("Invalid Base64 or encrypted sequence.");
            }

            return bytes.Slice(0, written);
        }
        public void SetCredentialPassword(EncryptedCredential credential, NetworkCredential networkCredential, ReadOnlySpan<char> encryptedPassword, Encoding encoding)
        {
            if (credential is not DpApiEncryptedCredential windowsCreds)
            {
                throw new InvalidDataException("On the windows platform, the credential must be a DpApiEncryptedCredential type.");
            }

            byte[] plainBytes = this.Decrypt(encryptedPassword, windowsCreds.DpapiScope);
            
            SecureString securePass = ReadInPlainBytes(plainBytes, encoding);

            networkCredential.SecurePassword = securePass;
            Array.Clear(plainBytes);
        }

        private IAccountName DecryptAccountName(ReadOnlySpan<char> encryptedChars, Encoding encoding, DataProtectionScope scope)
        {
            byte[] plainBytes = this.Decrypt(encryptedChars, scope);

            IAccountName accountName = AccountName.Parse(plainBytes, encoding);
            Array.Clear(plainBytes);

            return accountName;
        }
        private EncryptionResult<DpApiEncryptedCredential> Encrypt(DpApiEncryptedCredential credential, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(credential.UserName) && !string.IsNullOrWhiteSpace(credential.EncryptedUserName))
            {
                credential.UserAccountName = this.DecryptAccountName(credential.EncryptedUserName, encoding, credential.DpapiScope);
            }

            IReadOnlyList<ValidationResult> errors = ValidateAccountName(credential);
            if (errors.Count > 0)
            {
                return new EncryptionResult<DpApiEncryptedCredential>
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

            return new EncryptionResult<DpApiEncryptedCredential> { Credential = credential, Errors = errors };
        }
        private static bool IsEncrypted(EncryptedCredential credential)
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
