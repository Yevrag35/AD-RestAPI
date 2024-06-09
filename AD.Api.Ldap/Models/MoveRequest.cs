using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MoveRequest
    {
        [JsonIgnore]
        public ClaimsPrincipal? ClaimsPrincipal { get; set; }

        [JsonProperty("dn", Required = Required.Always)]
        [NotNull]
        public string? DistinguishedName { get; set; }

        [JsonProperty("dest", Required = Required.Always)]
        [NotNull]
        public string? DestinationPath { get; set; }

        [JsonIgnore]
        public string? Domain { get; set; }

        [JsonIgnore]
        [MemberNotNullWhen(true, nameof(DistinguishedName), nameof(DestinationPath))]
        public bool IsValid => !string.IsNullOrWhiteSpace(this.DistinguishedName) && !string.IsNullOrWhiteSpace(this.DestinationPath)
            && !StringComparer.CurrentCultureIgnoreCase.Equals(this.DistinguishedName, this.DestinationPath);
    }
}
