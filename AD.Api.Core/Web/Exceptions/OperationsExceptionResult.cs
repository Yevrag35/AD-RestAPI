using System.DirectoryServices.Protocols;
using RCode = System.DirectoryServices.Protocols.ResultCode;

namespace AD.Api.Core.Web
{
    public sealed class OperationsExceptionResult : ApiExceptionResult<DirectoryOperationException>
    {
        public override string Error { get; }
        public override RCode Result { get; }
        public override int ResultCode { get; }
        protected override bool IsClientFault { get; }

        public OperationsExceptionResult([DisallowNull] DirectoryOperationException exception)
            : base(exception)
        {
            this.Error = exception.Message;
            this.Result = exception.Response.ResultCode;
            this.ResultCode = (int)this.Result;
            this.IsClientFault = exception.Response.ResultCode == RCode.NoSuchObject
                                 ||
                                 exception.Response.ResultCode == RCode.NoSuchAttribute;
        }
    }
}

