using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public ApiBadRequestResult(ModelStateDictionary failedModelState)
            : base(new ModelStateErrorBody(failedModelState))
        {
            this.StatusCode = this.StaticStatusCode;
        }
    }
}

