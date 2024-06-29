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
using IAuthSvc = AD.Api.Core.Authentication.IAuthorizationService;

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
            if (!context.HttpContext.User.IsAuthenticated())
            {
                context.Result = new ForbidResult(JwtBearerDefaults.AuthenticationScheme);
                return;
            }

            var authSvc = context.HttpContext.RequestServices.GetRequiredService<IAuthSvc>();
            var enumStrings = context.HttpContext.RequestServices.GetRequiredService<IEnumStrings<AuthorizedRole>>();
            string domain = (string)context.HttpContext.Items[QueryDomainAttribute.ModelName]!;

            Claim? role = context.HttpContext.User.FindFirst(ClaimTypes.Role);
            if (role is null || !enumStrings.TryGetEnum(role.Value, out AuthorizedRole aRole) || !aRole.HasFlag(this.Role) && !authSvc.TryAddScopesToContext(context.HttpContext, this.Role))
            {
                context.Result = new ForbidResult(JwtBearerDefaults.AuthenticationScheme);   
            }
        }
    }
}
