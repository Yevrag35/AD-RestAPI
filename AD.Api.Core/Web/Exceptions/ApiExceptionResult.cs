using AD.Api.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Web
{
    public abstract class ApiExceptionResult : ApiResult
    {
        public abstract string Error { get; }
        public abstract ResultCode Result { get; }
        public abstract int ResultCode { get; }
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        protected abstract bool IsClientFault { get; }

        protected sealed override int GetStatusCode()
        {
            return this.IsClientFault
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status500InternalServerError;
        }
        protected sealed override Task SerializeAsync(Stream bodyStream, JsonSerializerOptions options, int statusCode, ActionContext context, CancellationToken cancellationToken)
        {
            return JsonSerializer
                .SerializeAsync(bodyStream, this, this.GetType(), options, cancellationToken);
        }
    }

    public abstract class ApiExceptionResult<T> : ApiExceptionResult where T : Exception
    {
        private readonly string[] _errors;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? ExtendedError => _errors.Length > 0 ? _errors : null;

        protected ApiExceptionResult([DisallowNull] T exception)
        {
            _errors = this.AddExtendedErrors(exception, [])?.ToArray() ?? [];
        }

        protected virtual List<string> AddExtendedErrors([DisallowNull] T exception, List<string> messages)
        {
            Exception? inner = exception.InnerException;
            while (inner is not null)
            {
                string msg = $"{inner.GetType().GetName()}: {inner.Message ?? "No message provided."}";
                messages.Add(msg);
            }

            return messages;
        }
    }
}

