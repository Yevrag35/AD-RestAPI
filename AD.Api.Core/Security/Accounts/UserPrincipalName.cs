using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AD.Api.Core.Security.Accounts
{
    public sealed class UserPrincipalName : AccountName
    {
        public required string Value { get; init; }

        public UserPrincipalName()
        {
        }
        [SetsRequiredMembers]
        public UserPrincipalName(string value)
        {
            this.Value = value;
        }

        public override void SetCredential(NetworkCredential credential)
        {
            credential.Domain = string.Empty;
            credential.UserName = this.Value;
        }

        /// <summary>
        /// Returns the username in the User Principal Name format.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> in the User Principal Name format: <c>UserName@Domain</c>.
        /// </returns>
        public override string ToString()
        {
            return this.Value;
        }

        protected override bool TryValidateName(ValidationContext context, [NotNullWhen(false)] out ValidationResult? badResult)
        {
            badResult = null;
            if (string.IsNullOrWhiteSpace(this.Value) || this.Value.Length <= 3)
            {
                badResult = new ValidationResult("User Principal Names must be in the format: UserName@Domain.", 
                    [nameof(this.Value)]);

                return false;
            }

            ReadOnlySpan<char> chars = this.Value.AsSpan();
            int atIndex = chars.IndexOf('@');
            if (atIndex <= 0)
            {
                badResult = new ValidationResult("User Principal names must be in the format: UserName@Domain.", 
                    [nameof(this.Value)]);

                return false;
            }

            chars = chars.Slice(atIndex);
            if (chars.Length <= 1)
            {
                badResult = new ValidationResult("User principal names must have a domain name.", [nameof(this.Value)]);
                return false;
            }

            return true;
        }
    }
}

