using AD.Api.Core;
using AD.Api.Core.Ldap;
using AD.Api.Core.Serialization.Json;
using AD.Api.Enums;
using AD.Api.Serialization.Json;
using Microsoft.Extensions.Primitives;
using System.Globalization;

namespace AD.Api.Middleware;

public class DomainReaderMiddleware
{
	private const string w = "The requested domain is not registered: {0}";
	private static readonly CompositeFormat s_notRegistered = CompositeFormat.Parse("The requested domain is not registered: {0}");
	private const int MAX_MSG_LENGTH = 256;
	//static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	private readonly RequestDelegate _next;
	private readonly IConnectionService _connections;
	private readonly IEnumStrings<ResultCode> _errorCodes;
	private readonly ILogger _logger;

	public DomainReaderMiddleware(RequestDelegate next, IConnectionService connections, IEnumStrings<ResultCode> enumStrings, ILogger<DomainReaderMiddleware> logger)
	{
		_next = next;
		_connections = connections;
		_errorCodes = enumStrings;
		_logger = logger;
	}

	public Task Invoke(HttpContext httpContext)
	{
		StringValues domain = httpContext.Request.Query[DomainQuery.DomainModelName];
		if (domain.Count > 0 && !_connections.RegisteredConnections.TryGetValue(domain[0], out var context))
		{
			return this.WriteErrorBodyAsync(httpContext, domain[0]);
		}
		else
		{
			context = _connections.RegisteredConnections[string.Empty];
		}

		httpContext.Items.TryAdd(DomainQuery.DomainModelName, context.DomainName);

		return _next(httpContext);
	}

	private async Task WriteErrorBodyAsync(HttpContext context, string? domain)
	{
		var options = context.RequestServices.GetRequiredService<IJsonOptions>();

		context.Response.StatusCode = StatusCodes.Status400BadRequest;

		context.Response.ContentType = JsonConstants.ContentTypeWithCharset;
		Utf8JsonWriter writer = new(context.Response.Body);

		await using (writer.ConfigureAwait(false))
		{
			this.WriteJsonBody(writer, domain, options.SerializerOptions, out string message);

			Task task = writer.FlushAsync(context.RequestAborted);
			_logger.LogWarning(message);

			await task.ConfigureAwait(false);
		}
	}

	private void WriteJsonBody(Utf8JsonWriter writer, string? domain, JsonSerializerOptions options, out string message)
	{
		writer.WriteStartObject();
		WorkingNamingPolicy policy = new(options);

		policy.WritePropertyName(writer, "Result"u8);
		writer.WriteStringValue(_errorCodes[ResultCode.Unavailable]);
		policy.WritePropertyName(writer, "ResultCode"u8);
		writer.WriteNumberValue((int)ResultCode.Unavailable);

		policy.WritePropertyName(writer, "Message"u8);
		WriteMessage(writer, domain, options, out message);

		writer.WriteEndObject();
	}
	private static void WriteMessage(Utf8JsonWriter writer, string? domain, JsonSerializerOptions options, out string message)
	{
		message = string.Format(CultureInfo.CurrentCulture, s_notRegistered, domain ?? "<null>");
		writer.WriteStringValue(message);
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
