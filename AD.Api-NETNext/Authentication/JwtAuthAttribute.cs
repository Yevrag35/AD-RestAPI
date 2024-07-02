using AD.Api.Core.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.Api.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class JwtAuthAttribute : Attribute, IAuthorizationFilter
    {
        private const string NONE = "None";
        private readonly AuthorizedRole _actualRole;

        public AuthorizedRole Role { get; }

        public JwtAuthAttribute(AuthorizedRole role)
        {
            this.Role = role;
            _actualRole = role;
        }
        public JwtAuthAttribute(AuthorizedRole role, bool possiblyScoped)
        {
            this.Role = possiblyScoped ? AuthorizedRole.None : role;
            _actualRole = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.RequestServices.GetService<IAuthorizer>()?.Authorize(context, _actualRole);
        }
    }
}
