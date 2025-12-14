using AD.Api.Core.Serialization.Json;
using MinimalJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace AD.Api.Core.Web;

public abstract class ApiResult : IActionResult, IResult
{
	public Task ExecuteResultAsync(ActionContext context)
	{
		IOptions<JsonOptions> jsonOptions = context.HttpContext.RequestServices
			.GetRequiredService<IOptions<JsonOptions>>();

		return this.ExecuteAsyncCore(context.HttpContext, jsonOptions.Value.JsonSerializerOptions, context.HttpContext.RequestAborted);
	}

	public Task ExecuteAsync(HttpContext httpContext)
	{
		IOptions<MinimalJsonOptions> jsonOptions = httpContext.RequestServices
			.GetRequiredService<IOptions<MinimalJsonOptions>>();

		return this.ExecuteAsyncCore(httpContext, jsonOptions.Value.SerializerOptions, httpContext.RequestAborted);
	}

	private async Task ExecuteAsyncCore(HttpContext context, JsonSerializerOptions options, CancellationToken token)
	{
		HttpResponse response = context.Response;
		response.ContentType = JsonConstants.ContentTypeWithCharset;
		response.StatusCode = this.GetResponseStatusCode();

		await this.SerializeAsync(
			bodyStream: response.Body,
			options: options,
			statusCode: response.StatusCode,
			httpContext: context,
			cancellationToken: token)
		.ConfigureAwait(false);
	}

	private int GetResponseStatusCode()
	{
		int statusCode = this.GetStatusCode();
		if (!IsStatusCodeValid(ref statusCode, out int originalCode))
		{
			Debug.Fail("Invalid status code: " + originalCode);
		}

		return statusCode;
	}
	protected abstract int GetStatusCode();
	protected static bool IsStatusCodeFailure(in int statusCode)
	{
		return statusCode < StatusCodes.Status100Continue
			|| statusCode >= StatusCodes.Status400BadRequest;
	}
	protected static bool IsStatusCodeSuccess(in int statusCode)
	{
		return statusCode >= StatusCodes.Status200OK
			&& statusCode < StatusCodes.Status400BadRequest;
	}
	protected static bool IsStatusCodeValid(ref int statusCode, out int originalCode)
	{
		originalCode = statusCode;
		switch (statusCode)
		{
			case < StatusCodes.Status100Continue:
				statusCode = StatusCodes.Status200OK;
				return false;

			case > 599:
				statusCode = StatusCodes.Status500InternalServerError;
				return false;

			default:
				return true;
		}
	}
	protected abstract Task SerializeAsync(Stream bodyStream, JsonSerializerOptions options, int statusCode, HttpContext httpContext, CancellationToken cancellationToken);
}

