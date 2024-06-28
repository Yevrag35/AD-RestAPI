using Microsoft.AspNetCore.Http;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Web
{
    public sealed class ApiBadRequestResult : ErrorObjectResult
    {
        protected override int StaticStatusCode => StatusCodes.Status400BadRequest;

        public ApiBadRequestResult(string message, ResultCode resultCode)
            : base(message, in resultCode)
        {
            this.StatusCode = this.StaticStatusCode;
        }
    }
}

