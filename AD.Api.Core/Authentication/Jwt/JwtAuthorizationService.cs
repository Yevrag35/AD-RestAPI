using AD.Api.Collections.Enumerators;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap;
using AD.Api.Enums;
using AD.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Frozen;
using System.Security.Claims;

namespace AD.Api.Core.Authentication.Jwt;

internal sealed partial class JwtAuthorizationService : IAuthorizer
{
	static readonly ForbidResult s_forbidden = new(JwtBearerDefaults.AuthenticationScheme);

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
			context.Result = s_forbidden;
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
			context.Result = s_forbidden;
			return;
		}

		if (userRole.HasFlag(role))
		{
			// Is authorized.
			return;
		}
		else if (!possiblyScoped || !this.TryAddScopesToContext(context.HttpContext, role))
		{
			context.Result = s_forbidden;
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

		string domain = ((string?)context.Items[DomainQuery.DomainModelName]) ?? string.Empty;
		string name = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

		if (distinguishedName.IsEmpty || distinguishedName.Count == 1)
		{
			return false;
		}

		Span<char> chars = stackalloc char[distinguishedName.Length];
		WorkingScope scope = distinguishedName.ToWorkingScope(domain, requiredRole, chars);

		return this.IsAuthorized(name, ref scope, context.RequestServices.GetRequiredService<ILogger<JwtAuthorizationService>>());
	}
	[Obsolete("Use IsAuthorized(HttpContext, DistinguishedName, out AuthorizedRole) instead.")]
	public bool IsAuthorizedByParent(HttpContext context, string? parentPath)
	{
		parentPath ??= string.Empty;
		if (!context.NeedsScoping(out AuthorizedRole requiredRole))
		{
			return true;
		}

		string domain = ((string?)context.Items[DomainQuery.DomainModelName]) ?? string.Empty;
		string name = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

		WorkingScope scope = new(domain, parentPath, requiredRole);
		return this.IsAuthorized(name, ref scope, context.RequestServices.GetRequiredService<ILogger<JwtAuthorizationService>>());
	}
	private bool IsAuthorized(string? userName, ref WorkingScope scope, ILogger logger)
	{
		if (scope.RequiredRole == AuthorizedRole.None)
		{
			return true;
		}

		if (string.IsNullOrWhiteSpace(userName) || !this.Users.TryGetValue(userName, out AuthorizedUser? user))
		{
			Log.UserNotFoundInLibrary(logger, userName ?? "<null>");
			return false;
		}

		if ((user.Roles & scope.RequiredRole) != 0)
		{
			return true;
		}

		ArrayRefEnumerator<string> enumerator = new(user.Scopes);
		bool flag = false;
		int index = -1;

		while (enumerator.MoveNext(flag, ref index))
		{
			flag = this.Scopes[enumerator.Current].IsAuthorized(ref scope);
		}

		if (flag)
		{
			AuthorizationScope winningScope = this.Scopes[user.Scopes[index]];

			Log.UserAuthorizedForScope(logger, userName, winningScope.Domain, winningScope.Base);
		}
		else if (logger.IsEnabled(LogLevel.Warning))
		{
			Log.UserUnauthorizedForScope(logger, userName, new(scope.DomainName), new(scope.DistinguishedName));
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

	private static partial class Log
	{
		[LoggerMessage(LogLevel.Warning, Message = "User {UserName} not found in the authorization library.")]
		internal static partial void UserNotFoundInLibrary(ILogger logger, string userName);

		[LoggerMessage(LogLevel.Information, Message = "User {UserName} authorized for scope: {ScopeDomain}/{ScopeBase}.")]
		internal static partial void UserAuthorizedForScope(ILogger logger, string userName, string scopeDomain, string scopeBase);

		[LoggerMessage(LogLevel.Warning, Message = "User {UserName} unauthorized for scope: {ScopeDomain} ({DistinguishedName}.")]
		internal static partial void UserUnauthorizedForScope(ILogger logger, string userName, string scopeDomain, string distinguishedName);
	}
}

public static class AuthorizationServiceExtensions
{
	public static IServiceCollection AddJwtAuthorizer(this IServiceCollection services)
	{
		return services.AddSingleton<IAuthorizer>(x => x.GetRequiredService<JwtAuthorizationService>());
	}
}

