using AD.Api.Validation;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace AD.Api.Core.Ldap.Passwords
{
    public sealed class PasswordOperationSettings
    {
        [SHA1Thumbprint]
        public required PasswordEncryptionOptions Encryption { get; init; }
        [ValidateEncoding(ErrorMessage = "The encoding must be a valid encoding name or left 'null' to use UTF-8.")]
        public required PasswordOperationOptions Changes { get; init; }
        [ValidateEncoding(ErrorMessage = "The encoding must be a valid encoding name or left 'null' to use UTF-8.")]
        public required PasswordOperationOptions Resets { get; init; }
    }

    public sealed class PasswordOperationOptions : IValidatableEncoding
    {
        public required bool Enabled { get; init; }
        public required PasswordEncodingOptions Encoding { get; init; }

        Expression<Func<object, string?>>? IValidatableProperty<string>.GetValidatableProperty()
        {
            return x => ((PasswordOperationOptions)x).Encoding.Type;
        }
    }

    public sealed class PasswordEncryptionOptions : IValidatableThumbprint
    {
        public required bool Required { get; init; }
        [Base64String]
        public string? AESSharedKey { get; set; }
        [SHA1Thumbprint]
        public string? SHA1Thumbprint { get; set; }

        Expression<Func<object, string?>> IValidatableProperty<string>.GetValidatableProperty()
        {
            return x => ((PasswordEncryptionOptions)x).SHA1Thumbprint;
        }
    }
    public sealed class PasswordEncodingOptions
    {
        private Encoding? _encoding;

        [ValidateEncoding(ErrorMessage = "The encoding must be a valid encoding name or left 'null' to use UTF-8.")]
        public string? Type { get; set; }
        public required bool Required { get; init; }

        public Encoding GetEncoding()
        {
            return _encoding ??= !string.IsNullOrWhiteSpace(this.Type)
                ? System.Text.Encoding.GetEncoding(this.Type)
                : System.Text.Encoding.UTF8;
        }
    }
}

