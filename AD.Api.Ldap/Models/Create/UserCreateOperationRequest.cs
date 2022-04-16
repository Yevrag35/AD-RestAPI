using AD.Api.Ldap.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class UserCreateOperationRequest : CreateOperationRequest
    {
        /// <summary>
        /// The Base64-encoded password to set on the account.
        /// </summary>
        [JsonProperty("password")]
        [DisplayName("Password")]
        [DefaultValue(null)]
        public string? Base64Password { get; set; }

        [JsonIgnore]
        private string? _sam;

        /// <summary>
        /// The <see cref="string"/> or <see cref="int"/> values for the user's UAC.
        /// </summary>
        /// <example>NormalUser, Disabled, PasswordNeverExpires</example>
        [JsonProperty("userAccountControl")]
        [DefaultValue("NormalUser, Disabled, PasswordNotRequired")]
        public string? UserAccountControlString { get; set; }

        /// <summary>
        /// The sAMAccountName property of the user account.  Can only be 20 characters long.
        /// </summary>
        /// <example>jdoe</example>
        [LdapProperty("sAMAccountName")]
        [DefaultValue(null)]
        public string? SamAccountName
        {
            get => _sam;
            set
            {
                if (value is null)
                    return;

                if (value.Length > 20)
                    value = value[..20];

                _sam = value;
            }
        }

        [JsonIgnore]
        public override CreationType Type { get; internal set; } = CreationType.User;

        [JsonIgnore]
        [LdapProperty("userAccountControl")]
        [DefaultValue(Ldap.UserAccountControl.NormalUser | Ldap.UserAccountControl.PasswordNotRequired | Ldap.UserAccountControl.Disabled)]
        public UserAccountControl? UserAccountControl
        {
            get => Enum.TryParse(this.UserAccountControlString, out UserAccountControl uac)
            ? uac
            : null;
        }

        protected override void AddToProperties()
        {
            if (string.IsNullOrWhiteSpace(this.SamAccountName))
                this.SamAccountName = this.CommonName;

            if (int.TryParse(this.UserAccountControlString, out int uacInt))
                this.UserAccountControlString = ((Ldap.UserAccountControl)uacInt).ToString();

            if (this.UserAccountControl.HasValue && !this.UserAccountControl.Value.HasFlag(Ldap.UserAccountControl.PasswordNotRequired)
                && string.IsNullOrWhiteSpace(this.Base64Password))
            {
                this.UserAccountControlString = $"{this.UserAccountControlString}, {Ldap.UserAccountControl.PasswordNotRequired}";
            }

            base.AddToProperties();
        }
    }
}
