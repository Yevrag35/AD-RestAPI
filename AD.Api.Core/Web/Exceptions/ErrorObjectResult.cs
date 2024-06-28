using Microsoft.AspNetCore.Mvc;
using RCode = System.DirectoryServices.Protocols.ResultCode;

namespace AD.Api.Core.Web
{
    [DebuggerStepThrough]
    public abstract class ErrorObjectResult : ObjectResult
    {
        private readonly ErrorBody _body;

        protected abstract int StaticStatusCode { get; }

        protected ErrorObjectResult(ErrorBody body)
            : base(body)
        {
            _body = body;
        }
        protected ErrorObjectResult(string message, in RCode resultCode)
            : this(GetBodyFromDetails(message, in resultCode))
        {
        }

        protected static ErrorBody GetBodyFromDetails(string message, in RCode result)
        {
            return new ErrorBody
            {
                Message = message ?? string.Empty,
                Result = result,
                ResultCode = (int)result,
            };
        }

        public sealed override void ExecuteResult(ActionContext context)
        {
            if (!ReferenceEquals(_body, this.Value))
            {
                this.Value = _body;
            }

            this.StatusCode = this.StaticStatusCode;
            base.ExecuteResult(context);
        }
        public sealed override Task ExecuteResultAsync(ActionContext context)
        {
            if (!ReferenceEquals(_body, this.Value))
            {
                this.Value = _body;
            }

            this.StatusCode = this.StaticStatusCode;
            return base.ExecuteResultAsync(context);
        }
    }
}

