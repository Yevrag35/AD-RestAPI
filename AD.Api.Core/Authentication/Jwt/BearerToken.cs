using System.Collections.Immutable;
using System.Text.Json.Serialization;
#if !DEBUG
using System.Text.Json.Serialization;
#endif

namespace AD.Api.Core.Authentication.Jwt;

public sealed class BearerToken
{
	public required DateTimeOffset Expires { get; set; }
	public required AuthorizedRole Roles { get; set; }
#if !DEBUG
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
#endif
	public required ImmutableArray<string> Scopes { get; set; }
	[JsonPropertyName("access_token")]
	public required string Token { get; set; }
}