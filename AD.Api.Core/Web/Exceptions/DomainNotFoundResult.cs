using System.Text.Json.Serialization;
using RCode = System.DirectoryServices.Protocols.ResultCode;

namespace AD.Api.Core.Web
{
    public sealed class DomainNotFoundResult : ApiExceptionResult
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? DomainRequested { get; }
        public override string Error { get; }
        public override RCode Result { get; }
        public override int ResultCode { get; }
        protected override bool IsClientFault { get; }

        public DomainNotFoundResult(string? domainSpecified)
        {
            this.DomainRequested = domainSpecified;
            this.Error = string.Format(Messages.Response_DomainNotFound, domainSpecified);
            this.Result = RCode.Unavailable;
            this.ResultCode = (int)this.Result;
            this.IsClientFault = !string.IsNullOrEmpty(domainSpecified);
        }
    }
}

