using AD.Api.Ldap.Operations;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.DirectoryServices;

namespace AD.Api.Ldap
{
    public interface ISuccessResult
    {
        [JsonProperty("message", Order = 2)]
        string? Message { get; }

        [JsonProperty("success", Order = 1)]
        bool Success { get; }
    }

    public interface ICreateResult : ISuccessResult
    { 
        [JsonProperty("dn")]
        string DistinguishedName { get; }
    }

    public interface IErroredResult : ISuccessResult
    {
        [JsonProperty("error", Order = 3)]
        ErrorDetails Error { get; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore,
        ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OperationResult : ISuccessResult, ICreateResult, IErroredResult
    {
        [JsonProperty("dn")]
        public string? DistinguishedName { get; set; }

        string ICreateResult.DistinguishedName => this.DistinguishedName ?? string.Empty;

        [JsonProperty("message", Order = 2)]
        public string? Message { get; set; }

        [JsonProperty("error", Order = 3)]
        public ErrorDetails? Error { get; init; }

        ErrorDetails IErroredResult.Error => this.Error ?? new();

        [JsonProperty("success", Order = 1, Required = Required.Always)]
        public bool Success { get; init; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore,
        ItemNullValueHandling = NullValueHandling.Ignore)]
    public record ErrorDetails
    {
        private readonly string? _extendedMsg;
        private readonly int? _errorCode;

        /// <summary>
        /// Additional details (if any) about the exception that occurred.
        /// </summary>
        /// <example></example>
        [JsonProperty("errorExtended", Order = 2)]
        public string? ExtendedMessage
        {
            get => _extendedMsg;
            init => _extendedMsg = value;
        }

        /// <summary>
        /// The error code returned.
        /// </summary>
        /// <example>-2147483648</example>
        [JsonProperty("errorCode", Order = 1)]
        public int? ErrorCode
        {
            get => _errorCode;
            init => _errorCode = value;
        }

        /// <summary>
        /// The LDAP attribute that caused the exception, if applicable.
        /// </summary>
        /// <example>mail</example>
        [JsonProperty("onProperty", Order = 3)]
        public string? Property { get; init; }

        /// <summary>
        /// The type of operation that was trying to be performed.
        /// </summary>
        [JsonProperty("operationType", Order = 4, Required = Required.Always)]
        public OperationType OperationType { get; set; } = OperationType.Commit;

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
