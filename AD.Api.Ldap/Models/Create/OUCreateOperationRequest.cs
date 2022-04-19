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
    public class OUCreateOperationRequest : CreateOperationRequest
    {
        [JsonProperty]
        [LdapProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("protect")]
        public bool ProtectFromAccidentalDeletion { get; set; }

        [JsonIgnore]
        public override CreationType Type { get; internal set; } = CreationType.OrganizationalUnit;
    }
}
