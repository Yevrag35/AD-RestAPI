using AD.Api.Core.Authentication.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.Api.Core.Authentication
{
    public interface IAuthorizer
    {
        //IReadOnlyDictionary<string, AuthorizationScope> Scopes { get; }
        //IReadOnlyDictionary<string, AuthorizedUser> Users { get; }

        void Authorize(AuthorizationFilterContext context, AuthorizedRole role);
        bool IsAuthorized(HttpContext context, string? parentPath);
        //bool IsAuthorized(string? userName, WorkingScope scope);
        //bool TryAddScopesToContext(HttpContext context, AuthorizedRole requiredRole);
    }
}

