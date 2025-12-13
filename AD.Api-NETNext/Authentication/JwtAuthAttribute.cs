using AD.Api.Core.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.Api.Authentication;

/// <summary>
/// Indicates that the specified <see cref="AuthorizedRole"/> is required to access the decorated controller or 
/// action.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class JwtAuthAttribute : Attribute, IAuthorizationFilter
{
	public bool PossiblyScoped { get; init; }
	public AuthorizedRole Role { get; }

	public JwtAuthAttribute(AuthorizedRole role)
	{
		this.Role = role;
	}

	public void OnAuthorization(AuthorizationFilterContext context)
	{
		context.HttpContext.RequestServices.GetService<IAuthorizer>()?.Authorize(context, this.Role, this.PossiblyScoped);
	}
}
