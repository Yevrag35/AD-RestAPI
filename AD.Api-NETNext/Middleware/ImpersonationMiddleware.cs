using AD.Api.Extensions;
using System.Security.Principal;

namespace AD.Api.Middleware
{
    public sealed class ImpersonationMiddleware
    {
        private readonly RequestDelegate _next;

        public ImpersonationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.TryGetWindowsIdentity(out WindowsIdentity? wid))
            {
                return _next(httpContext);
            }

            return InvokeImpersonatedAsync(httpContext, wid, _next);
        }

        private static async Task InvokeImpersonatedAsync(HttpContext context, WindowsIdentity identity, RequestDelegate next)
        {
            await WindowsIdentity.RunImpersonatedAsync(identity.AccessToken, async () =>
            {
                await next(context).ConfigureAwait(false);
            })
            .ConfigureAwait(false);
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
