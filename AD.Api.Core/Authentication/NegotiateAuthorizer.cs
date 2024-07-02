using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.Api.Core.Authentication
{
    public sealed class NegotiateAuthorizer : IAuthorizer
    {
        public void Authorize(AuthorizationFilterContext context, AuthorizedRole role)
        {
            return;
        }

        public bool IsAuthorized(HttpContext context, string? parentPath)
        {
            return true;
        }
    }
}

