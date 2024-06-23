using AD.Api.Core.Serialization.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AD.Api.Core.Web
{
    public abstract class ApiResult : IActionResult
    {
        public async Task ExecuteResultAsync(ActionContext context)
        {
            IOptions<JsonOptions> jsonOptions = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<JsonOptions>>();

            HttpResponse response = context.HttpContext.Response;
            response.ContentType = JsonConstants.ContentTypeWithCharset;
            response.StatusCode = this.GetResponseStatusCode();

            await this.SerializeAsync(response.Body, jsonOptions.Value.JsonSerializerOptions, response.StatusCode,
                context, context.HttpContext.RequestAborted)
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
        protected abstract Task SerializeAsync(Stream bodyStream, JsonSerializerOptions options, int statusCode, ActionContext context, CancellationToken cancellationToken);
    }
}

