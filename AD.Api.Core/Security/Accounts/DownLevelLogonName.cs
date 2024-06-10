using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AD.Api.Core.Security.Accounts
{
    public sealed class DownLevelLogonName : AccountName
    {
        private string? _combined;

        public required string Domain { get; init; }
        public required string UserName { get; init; }

        public DownLevelLogonName()
        {
        }
        [SetsRequiredMembers]
        public DownLevelLogonName(string domain, string userName)
        {
            this.Domain = domain;
            this.UserName = userName;
        }

        public override void SetCredential(NetworkCredential credential)
        {
            credential.Domain = this.Domain;
            credential.UserName = this.UserName;
        }

        /// <summary>
        /// Returns the username in the Down-level Logon Name format.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> in the Down-level Logon Name format: <c>Domain\UserName</c>.
        /// </returns>
        public override string ToString()
        {
            return _combined ??= $"{this.Domain}\\{this.UserName}";
        }

        protected override bool TryValidateName(ValidationContext context, [NotNullWhen(false)] out ValidationResult? badResult)
        {
            badResult = null;
            if (string.IsNullOrWhiteSpace(this.Domain) || string.IsNullOrWhiteSpace(this.UserName))
            {
                badResult = new ValidationResult("Down-level logon names require both a NetBIOS domain name and a SamAccountName.", [nameof(this.Domain), nameof(this.UserName)]);
                return false;
            }

            return true;
        }
    }
}

