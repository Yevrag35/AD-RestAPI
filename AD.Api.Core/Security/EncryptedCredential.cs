using AD.Api.Core.Security.Accounts;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Settings.Credentials;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
using Encode = System.Text.Encoding;

namespace AD.Api.Core.Security
{
    public abstract class EncryptedCredential : IValidatableObject, ILdapCredential
    {
        private bool _disposed;
        private NetworkCredential? _netCreds;
        private readonly Encode _encoding = Encode.UTF8;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string? Encoding
        {
            get => null;
            init
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                try
                {
                    _encoding = Encode.GetEncoding(value);
                }
                catch (ArgumentException)
                {
                    _encoding = Encode.UTF8;
                }
            }
        }
        internal bool IsEmpty { get; private set; }
        public string EncryptedPassword { get; set; } = string.Empty;
        public string EncryptedUserName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? UserName { get; set; }
        public IAccountName UserAccountName { get; set; } = AccountName.Empty;

        
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _netCreds?.SecurePassword?.Dispose();
                }

                _netCreds = null;
                _disposed = true;
            }
        }
        public static EncryptionResult<T> FromSettings<T>(IConfigurationSection connectionSection, IEncryptionService encryptionService) where T : EncryptedCredential
        {
            IConfigurationSection section = connectionSection.GetSection("Credentials");
            if (!section.Exists())
            {
                return EncryptionResult.Empty<T>();
            }

            T? creds = section.Get<T>(x => x.ErrorOnUnknownConfiguration = false);
            if (creds is null)
            {
                return EncryptionResult.Empty<T>();
            }

            if (!string.IsNullOrWhiteSpace(creds.UserName))
            {
                creds.EncryptedUserName = string.Empty;
                creds.UserAccountName = AccountName.Parse(creds.UserName);
            }

            ValidationResult[] results = creds.Validate(new ValidationContext(creds)).ToArray();
            return new EncryptionResult<T>
            {
                Credential = creds,
                Errors = results,
            };
        }
        public Encode GetEncoding()
        {
            return _encoding;
        }
        public virtual void SetCredential(LdapConnection connection)
        {
            connection.Credential = _netCreds;
        }
        [MemberNotNull(nameof(_netCreds))]
        public virtual void StoreCredential(IEncryptionService encryptionService)
        {
            if (this.IsEmpty)
            {
                _netCreds = new();
                return;
            }
            else if (_netCreds is not null)
            {
                return;
            }

            _netCreds = new();
            this.UserAccountName.SetCredential(_netCreds);
            encryptionService.SetCredentialPassword(this, _netCreds, this.EncryptedPassword, _encoding);
            this.Password = null;
            this.UserName = null;
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(this.EncryptedPassword) && string.IsNullOrWhiteSpace(this.Password))
            {
                yield return new ValidationResult("A password is required", 
                    [nameof(this.EncryptedPassword), nameof(this.Password)]);
            }

            if (string.IsNullOrWhiteSpace(this.EncryptedUserName) && string.IsNullOrWhiteSpace(this.UserName))
            {
                yield return new ValidationResult("A username is required",
                     [nameof(this.EncryptedUserName), nameof(this.UserName)]);
            }
        }

        #region EMPTY CREDENTIAL
        public static readonly EncryptedCredential NoCredential = new Empty();
        private sealed class Empty : EncryptedCredential
        {
            internal Empty()
            {
                this.IsEmpty = true;
            }

            public override void SetCredential(LdapConnection connection)
            {
            }
            public override void StoreCredential(IEncryptionService encryptionService)
            {
                _netCreds ??= new();
            }
        }

        #endregion
    }
}
