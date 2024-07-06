namespace AD.Api.Core.Authentication.Jwt;

public sealed class BearerToken
{
    public required DateTimeOffset Expires { get; set; }
    public required AuthorizedRole Roles { get; set; }
    public required string Token { get; set; }
}