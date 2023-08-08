using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace AD.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        public async Task Invoke(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex is null)
                return;

            var error = new
            {
                message = ex.Message
            };

            context.Response.ContentType = Strings.ContentType_Json;

            using (var writer = new StreamWriter(context.Response.Body))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.WriteRaw(JsonConvert.SerializeObject(error, Formatting.Indented));
                    await jsonWriter.FlushAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
