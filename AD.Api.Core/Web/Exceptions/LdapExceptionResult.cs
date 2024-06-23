using System.DirectoryServices.Protocols;
using RCode = System.DirectoryServices.Protocols.ResultCode;

namespace AD.Api.Core.Web
{
    public sealed class LdapExceptionResult : ApiExceptionResult<LdapException>
    {
        public override string Error { get; }
        public override RCode Result { get; }
        public override int ResultCode { get; }
        protected override bool IsClientFault { get; }

        public LdapExceptionResult(LdapException exception)
            : base(exception)
        {
            this.Error = exception.Message;
            this.Result = RCode.OperationsError;
            this.ResultCode = exception.ErrorCode;
            this.IsClientFault = true;
        }

        protected override List<string> AddExtendedErrors([DisallowNull] LdapException exception, List<string> messages)
        {
            base.AddExtendedErrors(exception, messages);

            if(!string.IsNullOrWhiteSpace(exception.ServerErrorMessage))
            {
                messages.Insert(0, $"Server message: {exception.ServerErrorMessage}");
            }

            return messages;
        }
    }
}

