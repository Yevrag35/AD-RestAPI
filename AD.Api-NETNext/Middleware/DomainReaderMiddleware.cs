using AD.Api.Core.Ldap;
using AD.Api.Core.Web.Attributes;
using Microsoft.Extensions.Primitives;

namespace AD.Api.Middleware
{
    public class DomainReaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConnectionService _connections;

        public DomainReaderMiddleware(RequestDelegate next, IConnectionService connections)
        {
            _next = next;
            _connections = connections;
        }

        public Task Invoke(HttpContext httpContext)
        {
            StringValues domain = httpContext.Request.Query[QueryDomainAttribute.ModelName];
            string domainKey = _connections.RegisteredConnections[domain.Count > 0 ? domain[0] : string.Empty].DomainName;
            
            httpContext.Items.TryAdd(QueryDomainAttribute.ModelName, domainKey);

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class DomainReaderMiddlewareExtensions
    {
        public static IApplicationBuilder UseDomainReaderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DomainReaderMiddleware>();
        }
    }
}
