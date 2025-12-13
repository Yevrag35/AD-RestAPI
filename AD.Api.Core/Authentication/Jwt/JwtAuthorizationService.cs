using AD.Api.Collections.Enumerators;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap;
using AD.Api.Enums;
using AD.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Frozen;
using System.Security.Claims;

namespace AD.Api.Core.Authentication.Jwt;

internal sealed class JwtAuthorizationService : IAuthorizer
{
	static readonly ForbidResult _forbidden = new(JwtBearerDefaults.AuthenticationScheme);
	static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public FrozenDictionary<string, AuthorizationScope> Scopes { get; }
	public FrozenDictionary<string, AuthorizedUser> Users { get; }
	public IEnumStrings<AuthorizedRole> RoleEnums { get; }

	public JwtAuthorizationService(FrozenDictionary<string, AuthorizationScope> scopes, FrozenDictionary<string, AuthorizedUser> users, IEnumStrings<AuthorizedRole> enumStrings)
	{
		this.Scopes = scopes;
		this.Users = users;
		this.RoleEnums = enumStrings;
	}

	public void Authorize(AuthorizationFilterContext context, AuthorizedRole role, bool possiblyScoped)
	{
		if (!(context.HttpContext.User.Identity?.IsAuthenticated).GetValueOrDefault())
		{
			context.Result = _forbidden;
			return;
		}
		else if (role == AuthorizedRole.None)
		{
			return;
		}

		if (!context.HttpContext.User.TryFindFirst(ClaimTypes.Role, out Claim? claim)
			||
			!this.RoleEnums.TryGetEnum(claim.Value, out AuthorizedRole userRole))
		{
			context.Result = _forbidden;
			return;
		}

		if (userRole.HasFlag(role))
		{
			// Is authorized.
			return;
		}
		else if (!possiblyScoped || !this.TryAddScopesToContext(context.HttpContext, role))
		{
			context.Result = _forbidden;
			return;
		}
	}
	private bool HasNoMatchingScopes(string[] scopes)
	{
		foreach (string scope in scopes)
		{
			if (this.Scopes.ContainsKey(scope))
			{
				return false;
			}
		}

		return true;
	}
	public bool IsAuthorized(HttpContext context, DistinguishedName distinguishedName, out AuthorizedRole requiredRole)
	{
		if (!context.NeedsScoping(out requiredRole) || requiredRole == AuthorizedRole.None)
		{
			return true;
		}

		string domain = ((string?)context.Items[DomainQuery.DomainModelName]).OrEmpty();
		string name = context.User.FindFirstValue(ClaimTypes.NameIdentifier).OrEmpty();

		if (distinguishedName.IsEmpty || distinguishedName.Count == 1)
		{
			return false;
		}

		Span<char> chars = stackalloc char[distinguishedName.Length];
		WorkingScope scope = distinguishedName.ToWorkingScope(domain, requiredRole, chars);

		return this.IsAuthorized(name, ref scope);
	}
	[Obsolete("Use IsAuthorized(HttpContext, DistinguishedName, out AuthorizedRole) instead.")]
	public bool IsAuthorizedByParent(HttpContext context, string? parentPath)
	{
		parentPath ??= string.Empty;
		if (!context.NeedsScoping(out AuthorizedRole requiredRole))
		{
			return true;
		}

		string domain = ((string?)context.Items[DomainQuery.DomainModelName]).OrEmpty();
		string name = context.User.FindFirstValue(ClaimTypes.NameIdentifier).OrEmpty();

		WorkingScope scope = new(domain, parentPath, requiredRole);
		return this.IsAuthorized(name, ref scope);
	}
	private bool IsAuthorized(string? userName, ref WorkingScope scope)
	{
		if (scope.RequiredRole == AuthorizedRole.None)
		{
			return true;
		}

		if (string.IsNullOrWhiteSpace(userName) || !this.Users.TryGetValue(userName, out AuthorizedUser? user))
		{
			_logger.Warn("User {Name} not found in the authorization library.", userName);
			return false;
		}

		if (user.Roles.HasFlag(scope.RequiredRole))
		{
			return true;
		}

		ArrayRefEnumerator<string> enumerator = new(user.Scopes);
		bool flag = false;
		int index = -1;

		while (enumerator.MoveNext(in flag, ref index))
		{
			flag = this.Scopes[enumerator.Current].IsAuthorized(ref scope);
		}

		if (flag)
		{
			AuthorizationScope winningScope = this.Scopes[user.Scopes[index]];

			_logger.Info("User {Name} authorized for scope: Domain: {Domain} - {Scope}.",
				userName, winningScope.Domain, winningScope.Roles);
		}
		else
		{
			_logger.Warn("User {Name} is not authorized for path: {Path:l} ({Domain:l})",
				user.UserName, scope.DistinguishedName.ToString(), scope.DomainName.ToString());
		}

		return flag;
	}
	private bool TryAddScopesToContext(HttpContext context, AuthorizedRole requiredRole)
	{
		if (!context.User.TryGetScopesFromClaim(AuthorizationScope.CLAIM_TYPE, out string[]? scopes)
			||
			this.HasNoMatchingScopes(scopes))
		{
			return false;
		}

		context.AddNeedsScoping(in requiredRole);
		return true;
	}
}

public static class AuthorizationServiceExtensions
{
	public static IServiceCollection AddJwtAuthorizer(this IServiceCollection services)
	{
		return services.AddSingleton<IAuthorizer>(x => x.GetRequiredService<JwtAuthorizationService>());
	}
}

