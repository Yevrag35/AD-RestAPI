using AD.Api.Core.Security;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Users;

public sealed record CreatedObject
{
	public required string Domain { get; init; }
	[JsonPropertyName("dn")]
	public required string DistinguishedName { get; init; }
	[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
	public string Location { get; init; } = string.Empty;
	public required SidString ObjectSid { get; init; }
}