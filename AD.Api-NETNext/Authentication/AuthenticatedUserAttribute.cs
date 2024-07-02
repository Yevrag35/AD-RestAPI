using AD.Api.Core.Authentication;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Core.Web.Attributes;
using AD.Api.Enums;
using AD.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace AD.Api.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AuthenticatedUserAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public AuthorizedRole Role { get; }
        public AuthenticatedUserAttribute(AuthorizedRole role)
            : base(role.ToString())
        {
            this.Role = role;
        }
        public AuthenticatedUserAttribute(AuthorizedRole role, bool possibleScoped)
             : base(possibleScoped ? nameof(AuthorizedRole.None) : role.ToString())
        {
            this.Role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.RequestServices.GetService<IAuthorizer>()?.Authorize(context, this.Role);
        }
    }
}
