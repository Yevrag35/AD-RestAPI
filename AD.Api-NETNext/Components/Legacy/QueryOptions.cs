using System.Security.Claims;

namespace AD.Api.Ldap.Search
{
    public record QueryOptions : SearchOptions
    {
        public ClaimsPrincipal? ClaimsPrincipal { get; set; }
        public string? Domain { get; init; }
        public string? SearchBase { get; init; }
    }
}
