using AD.Api.Exceptions;
using AD.Api.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using RCode = System.DirectoryServices.Protocols.ResultCode;

namespace AD.Api.Core.Web
{
    public abstract class ApiExceptionResult : ApiResult
    {
        public abstract string Error { get; }
        public abstract RCode Result { get; }
        public abstract int ResultCode { get; }

        protected override int GetStatusCode()
        {
            return GetStatusCodeFromResult(this.Result);
        }

        public static int GetStatusCodeFromResult(RCode resultCode)
        {
            switch (resultCode)
            {
                case RCode.AdminLimitExceeded:
                    return StatusCodes.Status429TooManyRequests;

                case RCode.AffectsMultipleDsas:
                case RCode.AliasDereferencingProblem:
                case RCode.AliasProblem:
                case RCode.Busy:
                case RCode.OffsetRangeError:
                case RCode.Other:
                    goto default;

                case RCode.AttributeOrValueExists:
                case RCode.ConstraintViolation:
                case RCode.NamingViolation:
                case RCode.NotAllowedOnNonLeaf:
                case RCode.NotAllowedOnRdn:
                case RCode.OperationsError:
                case RCode.ObjectClassViolation:
                case RCode.UnwillingToPerform:
                case RCode.VirtualListViewError:
                    return StatusCodes.Status422UnprocessableEntity;

                case RCode.AuthMethodNotSupported:
                    return StatusCodes.Status501NotImplemented;

                case RCode.ConfidentialityRequired:
                case RCode.InappropriateAuthentication:
                case RCode.ProtocolError:
                case RCode.SortControlMissing:
                    return StatusCodes.Status417ExpectationFailed;

                case RCode.EntryAlreadyExists:
                    return StatusCodes.Status409Conflict;

                case RCode.InappropriateMatching:
                case RCode.InvalidAttributeSyntax:
                case RCode.InvalidDNSyntax:
                case RCode.NoSuchAttribute:
                case RCode.UnavailableCriticalExtension:
                case RCode.UndefinedAttributeType:
                    return StatusCodes.Status400BadRequest;

                case RCode.InsufficientAccessRights:
                case RCode.ObjectClassModificationsProhibited:
                    return StatusCodes.Status403Forbidden;

                case RCode.LoopDetect:
                    return StatusCodes.Status508LoopDetected;

                case RCode.NoSuchObject:
                    return StatusCodes.Status410Gone;

                case RCode.Referral:
                case RCode.ReferralV2:
                case RCode.Unavailable:
                    return StatusCodes.Status421MisdirectedRequest;

                case RCode.ResultsTooLarge:
                case RCode.SizeLimitExceeded:
                    return StatusCodes.Status413RequestEntityTooLarge;

                case RCode.SaslBindInProgress:
                    return StatusCodes.Status419AuthenticationTimeout;

                case RCode.TimeLimitExceeded:
                    return StatusCodes.Status408RequestTimeout;

                case RCode.StrongAuthRequired:
                    return StatusCodes.Status428PreconditionRequired;

                default:
                    return StatusCodes.Status500InternalServerError;
            }
        }

        protected sealed override Task SerializeAsync(Stream bodyStream, JsonSerializerOptions options, int statusCode, ActionContext context, CancellationToken cancellationToken)
        {
            return JsonSerializer
                .SerializeAsync(bodyStream, this, this.GetType(), options, cancellationToken);
        }
    }

    public abstract class ApiExceptionResult<T> : ApiExceptionResult where T : Exception
    {
        private readonly string[]? _errors;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? ExtendedError => _errors;

        protected ApiExceptionResult([DisallowNull] T exception)
        {
            List<string> list = this.AddExtendedErrors(exception, []);
            _errors = list.Count > 0 ? [.. list] : [];
        }

        protected virtual List<string> AddExtendedErrors([DisallowNull] T exception, List<string> messages)
        {
            exception.GetAllMessages(messages);
            return messages;
        }
    }
}

