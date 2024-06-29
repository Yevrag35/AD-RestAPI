using AD.Api.Core.Ldap.Filters;
using AD.Api.Serialization.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using UAC = AD.Api.Core.Ldap.UserAccountControl;

namespace AD.Api.Core.Ldap.Users
{
    /// <summary>
    /// A request model for creating a new user in Active Directory.
    /// </summary>
    public sealed class CreateUserRequest : CreateBody
    {
        private const UAC DEFAULT_UAC = UAC.NormalUser | UAC.Disabled;
        private JsonDictionary _dict = null!;

        [JsonExtensionData]
        public IDictionary<string, object?> Attributes
        {
            get => _dict ??= new(1);
            set => _dict = new(value);
        }

        public string? Mail
        {
            get => (string?)this.Attributes[AttributeConstants.MAIL];
            init => this.Attributes[AttributeConstants.MAIL] = value;
        }

        public required string Name
        {
            get => (string)this.Attributes[AttributeConstants.NAME]!;
            init => this.Attributes[AttributeConstants.NAME] = value;
        }

        [MaxLength(20, ErrorMessage = "The sAMAccountName length must be 20 characters or less.")]
        public required string SamAccountName
        {
            get => (string)this.Attributes[AttributeConstants.SAM_ACCOUNT_NAME]!;
            init => this.Attributes[AttributeConstants.SAM_ACCOUNT_NAME] = value;
        }
        public UAC UserAccountControl
        {
            get
            {
                if (!this.Attributes.TryGetValue(AttributeConstants.USER_ACCOUNT_CONTROL, out object? value)
                    ||
                    value is not int uacInt)
                {
                    this.Attributes[AttributeConstants.USER_ACCOUNT_CONTROL] = DEFAULT_UAC;
                    return DEFAULT_UAC;
                }

                return (UAC)uacInt;
            }
            init => this.Attributes[AttributeConstants.USER_ACCOUNT_CONTROL] = (int)value;
        }
        public string UserPrincipalName
        {
            get => (string?)this.Attributes[AttributeConstants.USER_PRINCIPAL_NAME] ?? string.Empty;
            init => this.Attributes[AttributeConstants.USER_PRINCIPAL_NAME] = value ?? string.Empty;
        }

        [BindNever]
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public override FilteredRequestType RequestType => FilteredRequestType.User;

        public CreateUserRequest()
        {
            this.Mail = string.Empty;
            this.UserPrincipalName = string.Empty;
        }

        public bool TryGetAttributes([NotNullWhen(true)] out IReadOnlyDictionary<string, object?>? attributes)
        {
            attributes = _dict;
            return attributes is not null;
        }
        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsValidUserPrincipalName(this.UserPrincipalName))
            {
                yield return new(
                    "The userPrincipalName must be in an email-address format - i.e. - must contain an '@' character and include a domain name.", [nameof(this.UserPrincipalName)]);
            }

            foreach (ValidationResult result in base.Validate(validationContext))
            {
                yield return result;
            }
        }

        private static bool IsValidUserPrincipalName(ReadOnlySpan<char> userPrincipalName)
        {
            if (userPrincipalName.IsEmpty)
            {
                return true;
            }

            int index = userPrincipalName.LastIndexOf('@');
            return index >= 0 && index < userPrincipalName.Length;
        }
    }
}

