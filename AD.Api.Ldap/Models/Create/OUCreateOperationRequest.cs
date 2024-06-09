using Newtonsoft.Json;

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
