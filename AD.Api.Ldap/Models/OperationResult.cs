using AD.Api.Ldap.Operations;
using Newtonsoft.Json;
using System;
using System.DirectoryServices;

namespace AD.Api.Ldap
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore,
        ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OperationResult
    {
        [JsonProperty("message", Order = 2)]
        public string? Message { get; set; }

        [JsonProperty("error", Order = 3)]
        public ErrorDetails? Error { get; init; }

        [JsonProperty("success", Order = 1, Required = Required.Always)]
        public bool Success { get; init; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore,
        ItemNullValueHandling = NullValueHandling.Ignore)]
    public record ErrorDetails
    {
        private readonly string? _extendedMsg;
        private readonly int? _errorCode;

        [JsonProperty("errorExtended", Order = 2)]
        public string? ExtendedMessage
        {
            get => _extendedMsg;
            init => _extendedMsg = value;
        }

        [JsonProperty("errorCode", Order = 1)]
        public int? ErrorCode
        {
            get => _errorCode;
            init => _errorCode = value;
        }

        [JsonProperty("onProperty", Order = 3)]
        public string? Property { get; init; }

        [JsonProperty("operationType", Order = 4, Required = Required.Always)]
        public OperationType OperationType { get; init; }

        public ErrorDetails()
        {
        }
        public ErrorDetails(DirectoryServicesCOMException exception)
        {
            _extendedMsg = exception.ExtendedErrorMessage;
            _errorCode = exception.ErrorCode;
        }
    }
}
