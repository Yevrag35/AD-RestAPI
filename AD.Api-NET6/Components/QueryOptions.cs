using System.Security.Claims;

namespace AD.Api.Ldap.Search
{
    public record QueryOptions : SearchOptions
    {
        public string? Domain { get; init; }
        public ClaimsPrincipal? Principal { get; set; }
        public string? SearchBase { get; init; }
    }
}
