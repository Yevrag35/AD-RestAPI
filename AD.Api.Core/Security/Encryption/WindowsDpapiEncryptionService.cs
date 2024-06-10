using AD.Api.Core.Security.Accounts;
using AD.Api.Core.Settings.Credentials;
using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
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
        public DataProtectionScope Scope { get; private set; } = DataProtectionScope.CurrentUser;

        public EncryptionResult ReadCredentials(IConfigurationSection connectionSection)
        {
            EncryptionResult result = EncryptedCredential.FromSettings(connectionSection, this);
            if (result.HasCredential && result.Errors.Count <= 0)
            {
                result = this.Encrypt(result.Credential, result.Credential.GetEncoding());
            }

            return result;
        }

        public EncryptionResult Encrypt(EncryptedCredential credential, Encoding encoding)
        {
            if (credential is not DpApiEncryptedCredential windowsCreds)
            {
                throw new InvalidDataException("On the windows platform, the credential must be a DpApiEncryptedCredential type.");
            }

            this.Scope = windowsCreds.DpapiScope;

            if (string.IsNullOrWhiteSpace(windowsCreds.UserName) && !string.IsNullOrWhiteSpace(windowsCreds.EncryptedUserName))
            {
                windowsCreds.UserAccountName = DecryptAccountName(windowsCreds.EncryptedUserName, encoding, this.Scope);
            }

            IReadOnlyList<ValidationResult> errors = ValidateAccountName(windowsCreds);
            if (errors.Count > 0)
            {
                return new EncryptionResult
                {
                    Credential = windowsCreds,
                    Errors = errors,
                };
            }

            if (!IsEncrypted(windowsCreds))
            {
                throw new SecurityException("Need encrypted credentials for now.");
            }

            if (errors.Count <= 0)
            {
                windowsCreds.StoreCredential(this);

                windowsCreds.Password = null;
                windowsCreds.UserName = null;
            }

            return new EncryptionResult { Credential = windowsCreds, Errors = errors };
        }
        public void SetCredentialPassword(NetworkCredential credential, ReadOnlySpan<char> encryptedPassword, Encoding encoding)
        {
            byte[] encBytes = ReadOutEncryptedBytes(encryptedPassword);
            
            byte[] plainBytes = ProtectedData.Unprotect(encBytes, null, this.Scope);
            SecureString securePass = ReadInPlainBytes(plainBytes, encoding);

            credential.SecurePassword = securePass;
            Array.Clear(encBytes);
            Array.Clear(plainBytes);
        }
        
        private static string EncryptString(ReadOnlySpan<char> plainChars, Encoding encoding, DataProtectionScope scope)
        {
            int length = encoding.GetMaxByteCount(plainChars.Length);
            byte[] bytes = new byte[length];

            _ = encoding.GetBytes(plainChars, bytes);
            byte[] encBytes = ProtectedData.Protect(bytes, null, scope);

            string s = Convert.ToBase64String(bytes);
            Array.Clear(bytes);
            Array.Clear(encBytes);
            return s;
        }
        private static bool IsEncrypted(EncryptedCredential credential)
        {
            return !string.IsNullOrWhiteSpace(credential.EncryptedUserName) && !string.IsNullOrWhiteSpace(credential.EncryptedPassword);
        }
        private static IAccountName DecryptAccountName(ReadOnlySpan<char> encryptedChars, Encoding encoding, DataProtectionScope scope)
        {
            byte[] encryptedBytes = ReadOutEncryptedBytes(encryptedChars);
            byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, scope);

            IAccountName accountName = AccountName.Parse(plainBytes, encoding);
            Array.Clear(encryptedBytes);
            Array.Clear(plainBytes);

            return accountName;
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
        private static byte[] ReadOutEncryptedBytes(ReadOnlySpan<char> encryptedPassword)
        {
            int minLength = ((encryptedPassword.Length * 3) + 3) / 4;
            Span<byte> span = stackalloc byte[minLength];

            if (!Convert.TryFromBase64Chars(encryptedPassword, span, out int written))
            {
                throw new FormatException("Invalid base64 sequence.");
            }

            return span.Slice(0, written).ToArray();    // allocates
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
                : [];
        }
    }
}
