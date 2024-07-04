using AD.Api.Core.Security.Accounts;
using AD.Api.Core.Security.Encryption;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
using System.Net;
using Encode = System.Text.Encoding;

namespace AD.Api.Core.Security
{
    /// <summary>
    /// Represents an abstract base class for handling encrypted credentials.
    /// </summary>
    /// <remarks>
    /// This class provides methods for storing, validating, and disposing of encrypted credentials,
    /// as well as converting settings to credentials using encryption services.
    /// </remarks>
    public abstract class EncryptedCredential : IValidatableObject, ILdapCredential
    {
        private bool _disposed;
        private NetworkCredential? _netCreds;
        private readonly Encode _encoding = Encode.UTF8;

        /// <summary>
        /// Gets or initializes the encoding used for the credentials.
        /// </summary>
        /// <value>
        /// A string representing the encoding. Defaults to UTF-8 if not specified or if an invalid encoding is provided.
        /// </value>
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

        /// <summary>
        /// Gets a value indicating whether the credential is empty.
        /// </summary>
        internal bool IsEmpty { get; private set; }

        /// <summary>
        /// Gets or sets the encrypted password.
        /// </summary>
        /// <value>
        /// A string representing the encrypted password.
        /// </value>
        public string EncryptedPassword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the encrypted username.
        /// </summary>
        /// <value>
        /// A string representing the encrypted username.
        /// </value>
        public string EncryptedUserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the plain text password.
        /// </summary>
        /// <value>
        /// A string representing the plain text password.
        /// </value>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the plain text username.
        /// </summary>
        /// <value>
        /// A string representing the plain text username.
        /// </value>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the user account name.
        /// </summary>
        /// <value>
        /// An <see cref="IAccountName"/> representing the user account name.
        /// </value>
        public IAccountName UserAccountName { get; set; } = AccountName.Empty;

        /// <summary>
        /// Disposes the current instance of <see cref="EncryptedCredential"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="EncryptedCredential"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

        /// <summary>
        /// Creates an <see cref="EncryptionResult{T}"/> from the provided settings.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="EncryptedCredential"/>.</typeparam>
        /// <param name="connectionSection">The configuration section containing the connection settings.</param>
        /// <param name="encryptionService">The encryption service used for encrypting the credentials.</param>
        /// <returns>An <see cref="EncryptionResult{T}"/> containing the credentials and any validation errors.</returns>
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

        /// <summary>
        /// Gets the encoding used for the credentials.
        /// </summary>
        /// <returns>An <see cref="Encode"/> representing the encoding.</returns>
        public Encode GetEncoding()
        {
            return _encoding;
        }

        /// <summary>
        /// Sets the credentials for the provided LDAP connection.
        /// </summary>
        /// <param name="connection">The LDAP connection to set the credentials for.</param>
        public virtual void SetCredential(LdapConnection connection)
        {
            connection.Credential = _netCreds;
        }

        /// <summary>
        /// Stores the credentials using the provided encryption service.
        /// </summary>
        /// <param name="encryptionService">The encryption service used for storing the credentials.</param>
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

        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="validationContext">The context information about the object being validated.</param>
        /// <returns>A collection of <see cref="ValidationResult"/> indicating validation errors.</returns>
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
        /// <summary>
        /// Represents an empty credential.
        /// </summary>
        public static readonly EncryptedCredential NoCredential = new Empty();

        private sealed class Empty : EncryptedCredential
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Empty"/> class.
            /// </summary>
            internal Empty()
            {
                this.IsEmpty = true;
            }

            /// <summary>
            /// Sets the credentials for the provided LDAP connection. No operation for empty credentials.
            /// </summary>
            /// <param name="connection">The LDAP connection to set the credentials for.</param>
            public override void SetCredential(LdapConnection connection)
            {
            }

            /// <summary>
            /// Stores the credentials using the provided encryption service. Initializes a new <see cref="NetworkCredential"/> if not already set.
            /// </summary>
            /// <param name="encryptionService">The encryption service used for storing the credentials.</param>
            public override void StoreCredential(IEncryptionService encryptionService)
            {
                _netCreds ??= new();
            }
        }

        #endregion
    }
}