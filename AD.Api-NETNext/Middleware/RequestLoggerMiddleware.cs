using AD.Api.Core.Extensions;
using AD.Api.Enums;
using NLog;
using System.Diagnostics;
using System.Net;

namespace AD.Api.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class RequestLoggerMiddleware
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly RequestDelegate _next;
        private readonly IEnumStrings<HttpStatusCode> _codes;

        public RequestLoggerMiddleware(RequestDelegate next, IEnumStrings<HttpStatusCode> codes)
        {
            _codes = codes;
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            bool added = httpContext.AddLogTraceId();
            Debug.Assert(added && httpContext.Response.Headers.ContainsKey(HttpContextExtensions.TRACE_ID_HEADER), 
                "Why didn't the trace ID get added?");

            await _next(httpContext).ConfigureAwait(false);

            string codeName = _codes[(HttpStatusCode)httpContext.Response.StatusCode];

            _logger.Info("{StatusCodeName:l}", codeName);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RequestLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLoggerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggerMiddleware>();
        }
    }
}
