using RCode = System.DirectoryServices.Protocols.ResultCode;

namespace AD.Api.Core.Web;

public sealed class LdapObjectMissingResult : ApiExceptionResult
{
    public override string Error { get; }
    public override ResultCode Result => RCode.NoSuchObject;
    public override int ResultCode => (int)this.Result;

    public LdapObjectMissingResult(string message)
    {
        this.Error = message;
    }
}