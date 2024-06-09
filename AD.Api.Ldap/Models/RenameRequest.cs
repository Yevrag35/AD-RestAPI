using Newtonsoft.Json;
using System.Security.Claims;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public record RenameRequest
    {
        [JsonIgnore]
        public ClaimsPrincipal? ClaimsPrincipal { get; set; }

        [JsonProperty("dn", Required = Required.Always)]
        public string DistinguishedName { get; set; } = string.Empty;

        [JsonIgnore]
        public string? Domain { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string NewName { get; set; } = string.Empty;
    }
}
