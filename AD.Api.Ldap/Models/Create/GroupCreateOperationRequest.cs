using AD.Api.Ldap.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore)]
    public class GroupCreateOperationRequest : CreateOperationRequest
    {
        [JsonIgnore]
        [LdapProperty("grouptype")]
        public GroupType GroupTypeValue = Ldap.GroupType.Security;

        [JsonIgnore]
        private string? _sam;

        [JsonProperty("type")]
        [DefaultValue("distribution")]
        public string? GroupType { get; set; } = "distribution";

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

        [JsonProperty]
        [DefaultValue("global")]
        public string? Scope { get; set; }

        [JsonIgnore]
        public override CreationType Type { get; internal set; } = CreationType.Group;

        protected override void AddToProperties()
        {
            if (string.IsNullOrWhiteSpace(this.SamAccountName))
                this.SamAccountName = this.CommonName;

            bool addedScope = false;
            if (!string.IsNullOrWhiteSpace(this.Scope) && Enum.TryParse(this.Scope, true, out Ldap.GroupType scope))
            {
                this.GroupTypeValue |= scope;
                addedScope = true;
            }
            else
                this.GroupTypeValue |= Ldap.GroupType.Global;

            if (!string.IsNullOrWhiteSpace(this.GroupType))
            {
                if (!this.GroupType.Equals(nameof(Ldap.GroupType.Security), StringComparison.CurrentCultureIgnoreCase)
                    && !this.GroupType.Equals("distribution", StringComparison.CurrentCultureIgnoreCase))
                    throw new ArgumentException($"'{this.GroupType}' is not a valid group type.");

                else if (this.GroupType.Equals("distribution", StringComparison.CurrentCultureIgnoreCase))
                {
                    this.GroupTypeValue &= ~Ldap.GroupType.Security;
                    if (!addedScope)
                    {
                        this.GroupTypeValue &= ~Ldap.GroupType.Global;
                        this.GroupTypeValue |= Ldap.GroupType.Universal;
                    }
                }
            }

            base.AddToProperties();
        }
    }
}
