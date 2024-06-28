using RCode = System.DirectoryServices.Protocols.ResultCode;

namespace AD.Api.Core.Web
{
    public class ErrorBody
    {
        public required string Message { get; set; }
        public required RCode Result { get; set; }
        public required int ResultCode { get; set; }
    }
}

