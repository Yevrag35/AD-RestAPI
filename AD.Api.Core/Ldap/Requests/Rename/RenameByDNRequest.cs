namespace AD.Api.Core.Ldap.Requests;

public sealed class RenameByDNRequest : RenameRequest, IScopedRequest
{
	[JsonPropertyName("dn")]
	public required DistinguishedName DistinguishedName { get; init; }

	public DistinguishedName GetScopedPath()
	{
		return this.DistinguishedName;
	}
	public string GetScopedPathMemberName()
	{
		return nameof(this.DistinguishedName);
	}
}
