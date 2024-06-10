using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AD.Api.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MultipleSchemaAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public MultipleSchemaAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var principal = new ClaimsPrincipal();
            bool azureAuthed = false;

            var resultAzureAD = await httpContext.AuthenticateAsync();
            if (resultAzureAD?.Principal is not null)
            {
                principal.AddIdentities(resultAzureAD.Principal.Identities);
                azureAuthed = true;
            }
            
            if (!azureAuthed)
            {
                var resultAuth0 = await httpContext.AuthenticateAsync("Auth0");
                if (resultAuth0?.Principal is not null)
                {
                    principal.AddIdentities(resultAuth0.Principal.Identities);
                }
            }

            httpContext.User = principal;

            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MultipleSchemaAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseMultipleSchemaAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MultipleSchemaAuthenticationMiddleware>();
        }
    }
}
