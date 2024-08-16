using AD.Api.Exceptions;
using AD.Api.Core.Ldap.Passwords;
using System.Security;
using System.Text;
using System.Runtime.InteropServices;
using AD.Api.Core.Ldap;
using AD.Api.Statics;

namespace AD.Api.Core.Security.Encryption
{
    public interface IPassHandler
    {
        void EncodePasswordChange(ReadOnlySpan<char> oldPassword, ReadOnlySpan<char> newPassword, ModifyRequest request);
        void EncodePasswordReset(ReadOnlySpan<char> password, ModifyRequest request);
    }

    internal abstract class PasswordHandler : IPassHandler
    {
        protected Encoding ChangeEncoding { get; }
        protected Encoding ResetEncoding { get; }
        protected PasswordOperationSettings Settings { get; }
        protected bool ChangesRequiresEncoding => this.Settings.Changes.Encoding.Required;
        protected bool ResetsRequiresEncoding => this.Settings.Resets.Encoding.Required;

        protected PasswordHandler(PasswordOperationSettings settings)
        {
            this.Settings = settings;
            this.ChangeEncoding = settings.Changes.Encoding.GetEncoding();
            this.ResetEncoding = settings.Resets.Encoding.GetEncoding();
        }

        public void EncodePasswordChange(ReadOnlySpan<char> oldPassword, ReadOnlySpan<char> newPassword, ModifyRequest request)
        {
            DirectoryAttributeModification removePass = new DirectoryAttributeModification
            {
                Name = AttributeConstants.UNICODE_PW,
                Operation = DirectoryAttributeOperation.Delete,
            };

            this.AddEncodedPasswordToModification(oldPassword, removePass, isChange: true);
            _ = request.Modifications.Add(removePass);

            DirectoryAttributeModification addPass = new DirectoryAttributeModification
            {
                Name = AttributeConstants.UNICODE_PW,
                Operation = DirectoryAttributeOperation.Add,
            };

            this.AddEncodedPasswordToModification(newPassword, addPass, isChange: true);
            _ = request.Modifications.Add(addPass);
        }
        public void EncodePasswordReset(ReadOnlySpan<char> password, ModifyRequest request)
        {
            DirectoryAttributeModification replacePass = new DirectoryAttributeModification
            {
                Name = AttributeConstants.UNICODE_PW,
                Operation = DirectoryAttributeOperation.Replace,
            };

            this.AddEncodedPasswordToModification(password, replacePass, isChange: false);
            _ = request.Modifications.Add(replacePass);
        }

        public SecureString DecryptChangePassword(ReadOnlySpan<char> password)
        {
            return this.Decrypt(password, this.ChangeEncoding);
        }
        public SecureString DecryptResetPassword(ReadOnlySpan<char> password)
        {
            return this.Decrypt(password, this.ResetEncoding);
        }

        private void AddEncodedPasswordToModification(ReadOnlySpan<char> password, DirectoryAttributeModification modification, bool isChange)
        {
            using SecureString secureString = isChange
                ? this.DecryptChangePassword(password)
                : this.DecryptResetPassword(password);

            Span<char> chars = stackalloc char[secureString.Length + 2];

            int written = GetSecureCharSpan(secureString, chars.Slice(1));
            Debug.Assert(written + 2 == chars.Length);

            chars[0] = CharConstants.DOUBLE_QUOTE;
            chars[^1] = CharConstants.DOUBLE_QUOTE;

            int length = Encoding.Unicode.GetByteCount(chars);
            byte[] passBytes = new byte[length];

            written = Encoding.Unicode.GetBytes(chars, passBytes);
            Debug.Assert(written == length);

            modification.Add(passBytes);
        }
        private static int GetSecureCharSpan(SecureString secureString, scoped Span<char> buffer)
        {
            nint bstrPointer = nint.Zero;
            try
            {
                bstrPointer = Marshal.SecureStringToBSTR(secureString);

                ReadOnlySpan<char> chars;
                unsafe
                {
                    chars = new ReadOnlySpan<char>(bstrPointer.ToPointer(), secureString.Length);
                }
                
                chars.CopyTo(buffer);
                return chars.Length;
            }
            finally
            {
                if (bstrPointer != nint.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstrPointer);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="AdApiException"/>
        protected abstract SecureString Decrypt(ReadOnlySpan<char> password, Encoding encoding);

        internal static PasswordHandler CreateService(PasswordOperationSettings settings)
        {
            if (!settings.Encryption.Required)
            {
                return new PasswordDecoder(settings);
            }

            if (!string.IsNullOrWhiteSpace(settings.Encryption.SHA1Thumbprint))
            {
                return new PasswordCertificateDecryptor(settings);
            }
            else
            {
                //TODO: AES Shared Key implementation
                throw new NotSupportedException("AES Shared Key decryption is not yet supported.");
            }
        }
    }
}

