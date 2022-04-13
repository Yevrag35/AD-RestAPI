using AD.Api.Ldap.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class UserCreateOperationRequest : CreateOperationRequest
    {
        [JsonIgnore]
        private string? _sam;

        [JsonProperty("userAccountControl")]
        private string? _userAccountControl;

        public string? Password { get; set; }

        [LdapProperty("sAMAccountName")]
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
        public UserAccountControl? UserAccountControl => Enum.TryParse(_userAccountControl, out UserAccountControl uac)
            ? uac
            : null;

        protected override void AddToProperties()
        {
            if (string.IsNullOrWhiteSpace(this.SamAccountName))
                this.SamAccountName = this.CommonName;

            if (this.UserAccountControl.HasValue && !this.UserAccountControl.Value.HasFlag(Ldap.UserAccountControl.PasswordNotRequired)
                && string.IsNullOrWhiteSpace(this.Password))
            {
                _userAccountControl = $"{_userAccountControl}, {Ldap.UserAccountControl.PasswordNotRequired}";
            }

            base.AddToProperties();
        }
    }
}
