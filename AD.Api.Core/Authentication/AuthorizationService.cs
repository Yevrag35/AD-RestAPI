using AD.Api.Attributes.Services;
using AD.Api.Collections.Enumerators;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Core.Extensions;
using AD.Api.Core.Web.Attributes;
using AD.Api.Enums;
using Microsoft.AspNetCore.Http;
using NLog;
using System.Collections.Frozen;
using System.Security.Claims;

namespace AD.Api.Core.Authentication
{
    public interface IAuthorizationService
    {
        IReadOnlyDictionary<string, AuthorizationScope> Scopes { get; }
        IReadOnlyDictionary<string, AuthorizedUser> Users { get; }

        bool IsAuthorized(HttpContext context, string? parentPath);
        bool IsAuthorized(string? userName, WorkingScope scope);
        bool TryAddScopesToContext(HttpContext context, AuthorizedRole requiredRole);
    }

    [DependencyRegistration(typeof(IAuthorizationService), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class AuthorizationService : IAuthorizationService
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public FrozenDictionary<string, AuthorizationScope> Scopes { get; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IReadOnlyDictionary<string, AuthorizationScope> IAuthorizationService.Scopes => this.Scopes;

        public FrozenDictionary<string, AuthorizedUser> Users { get; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IReadOnlyDictionary<string, AuthorizedUser> IAuthorizationService.Users => this.Users;

        public IEnumStrings<AuthorizedRole> RoleEnums { get; }

        public AuthorizationService(FrozenDictionary<string, AuthorizationScope> scopes, FrozenDictionary<string, AuthorizedUser> users, IEnumStrings<AuthorizedRole> enumStrings)
        {
            this.Scopes = scopes;
            this.Users = users;
            this.RoleEnums = enumStrings;
        }

        public bool TryAddScopesToContext(HttpContext context, AuthorizedRole requiredRole)
        {
            Claim? scopeClaim = context.User.FindFirst(AuthorizationScope.CLAIM_TYPE);
            if (scopeClaim is null)
            {
                return false;
            }

            string[] scopes = scopeClaim.Value.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            AuthorizationScope[] authScopes = new AuthorizationScope[scopes.Length];

            for (int i = 0; i < scopes.Length; i++)
            {
                authScopes[i] = this.Scopes[scopes[i]];
            }

            if (context.Items.TryAdd(AuthorizationScope.CLAIM_TYPE, authScopes))
            {
                context.AddNeedsScoping(in requiredRole);
                return true;
            }

            return false;
        }

        public bool IsAuthorized(HttpContext context, string? parentPath)
        {
            if (!context.NeedsScoping(out AuthorizedRole requiredRole))
            {
                return true;
            }

            string domain = (string?)context.Items[QueryDomainAttribute.ModelName] ?? string.Empty;

            string name = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            WorkingScope scope = new(domain, parentPath ?? string.Empty, requiredRole);
            return this.IsAuthorized(name, scope);
        }
        public bool IsAuthorized(string? userName, WorkingScope scope)
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
                flag = this.Scopes[enumerator.Current].IsAuthorized(in scope);
            }

            if (flag)
            {
                AuthorizationScope winningScope = this.Scopes[user.Scopes[index]];
                _logger.Info("User {Name} authorized for scope: Domain: {Domain} - {Scope}.", userName, winningScope.Domain, winningScope.Roles);
            }

            return flag;
        }
    }
}

