using AD.Api.Core.Ldap;
using AD.Api.Core.Serialization.Json;
using AD.Api.Core.Web;
using AD.Api.Core.Web.Attributes;
using AD.Api.Enums;
using AD.Api.Serialization.Json;
using AD.Api.Spans;
using AD.Api.Statics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NLog;
using System.Buffers;
using System.DirectoryServices.Protocols;
using System.Text.Json;

namespace AD.Api.Middleware
{
    public class DomainReaderMiddleware
    {
        private const string NOT_REGISTERED = "The requested domain is not registered: {DomainName}";
        private const int MAX_MSG_LENGTH = 256;
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly RequestDelegate _next;
        private readonly IConnectionService _connections;
        private readonly IEnumStrings<ResultCode> _errorCodes;

        public DomainReaderMiddleware(RequestDelegate next, IConnectionService connections, IEnumStrings<ResultCode> enumStrings)
        {
            _next = next;
            _connections = connections;
            _errorCodes = enumStrings;
        }

        public Task Invoke(HttpContext httpContext)
        {
            StringValues domain = httpContext.Request.Query[QueryDomainAttribute.ModelName];
            if (domain.Count > 0 && !_connections.RegisteredConnections.TryGetValue(domain[0], out var context))
            {
                return this.WriteErrorBodyAsync(httpContext, domain[0]); 
            }
            else
            {
                context = _connections.RegisteredConnections[string.Empty];
            }

            httpContext.Items.TryAdd(QueryDomainAttribute.ModelName, context.DomainName);

            return _next(httpContext);
        }

        private async Task WriteErrorBodyAsync(HttpContext context, string? domain)
        {
            //string msg = $"The requested domain is not registered: \"{domain}\".";
            _logger.Warn(NOT_REGISTERED, domain);
            var options = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            context.Response.ContentType = JsonConstants.ContentTypeWithCharset;
            await using Utf8JsonWriter writer = new(context.Response.Body);

            this.WriteJsonBody(writer, domain, options.Value.JsonSerializerOptions);

            await writer.FlushAsync(context.RequestAborted).ConfigureAwait(false);
        }

        private void WriteJsonBody(Utf8JsonWriter writer, ReadOnlySpan<char> domain, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WorkingNamingPolicy policy = new(options);

            policy.WritePropertyName(writer, "Result"u8);
            writer.WriteStringValue(_errorCodes[ResultCode.Unavailable]);
            policy.WritePropertyName(writer, "ResultCode"u8);
            writer.WriteNumberValue((int)ResultCode.Unavailable);

            policy.WritePropertyName(writer, "Message"u8);
            WriteMessage(writer, domain, options);

            writer.WriteEndObject();
        }
        private static void WriteMessage(Utf8JsonWriter writer, ReadOnlySpan<char> domain, JsonSerializerOptions options)
        {
            ReadOnlySpan<char> msg = NOT_REGISTERED;
            msg = msg.Slice(0, msg.IndexOf(['{'], StringComparison.Ordinal));
            int length = msg.Length + domain.Length + 2;

            char[]? array = null;
            bool isRented = false;

            Span<char> buffer = length < MAX_MSG_LENGTH
                ? stackalloc char[length]
                : SpanExtensions.RentArray(in length, ref isRented, ref array);

            msg.CopyTo(buffer);
            int pos = msg.Length;
            buffer[pos++] = CharConstants.DOUBLE_QUOTE;
            domain.CopyToSlice(buffer, ref pos);
            buffer[pos++] = CharConstants.DOUBLE_QUOTE;

            writer.WriteStringValue(buffer.Slice(0, pos));

            if (isRented)
            {
                ArrayPool<char>.Shared.Return(array!);
            }
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
