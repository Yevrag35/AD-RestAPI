using AD.Api.Core.Ldap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.Api.Core.Authentication;

public sealed class NegotiateAuthorizer : IAuthorizer
{
	public void Authorize(AuthorizationFilterContext context, AuthorizedRole role, bool possiblyScoped)
	{
		return;
	}
	public bool IsAuthorized(HttpContext context, DistinguishedName distinguishedName, out AuthorizedRole requiredRole)
	{
		requiredRole = AuthorizedRole.None;
		return true;
	}
	public bool IsAuthorizedByParent(HttpContext context, string? parentPath)
	{
		return true;
	}
}

