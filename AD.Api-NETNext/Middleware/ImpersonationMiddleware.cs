using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;

namespace AD.Api.Middleware
{
    public sealed class ImpersonationMiddleware
    {
        private readonly RequestDelegate _next;

        public ImpersonationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var user = (WindowsIdentity)httpContext.User.Identity!;
            

            Debug.WriteLine($"User: {user.Name}\tState: {user.ImpersonationLevel}\n");

            try
            {
                await WindowsIdentity.RunImpersonatedAsync(user.AccessToken, async () =>
                {
                    var impersonated = WindowsIdentity.GetCurrent();
                    Debug.WriteLine($"User: {impersonated.Name}\tState: {impersonated.ImpersonationLevel}\n");

                    await _next(httpContext);
                });
            }
            catch (Exception e)
            {
                await httpContext.Response.WriteAsync(e.ToString());
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ImpersonationMiddlewareExtensions
    {
        public static IApplicationBuilder UseImpersonationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ImpersonationMiddleware>();
        }
    }
}
