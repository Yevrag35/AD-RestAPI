using AD.Api.Core.Ldap;
using System.Text.Json.Serialization;

namespace AD.Api.Models
{
    public sealed class WellKnownPathResult
    {
        [JsonPropertyOrder(int.MaxValue)]
        public required DistinguishedName DistinguishedName { get; init; }
        public required WellKnownObjectValue WellKnown { get; init; }
    }
}
