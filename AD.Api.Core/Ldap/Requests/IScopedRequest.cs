namespace AD.Api.Core.Ldap.Requests;

public interface IScopedRequest
{
	DistinguishedName GetScopedPath();
	string GetScopedPathMemberName();
}