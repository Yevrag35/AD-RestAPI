using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Operations;
using AD.Api.Core.Schema;
using AD.Api.Core.Security;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Core.Ldap.Users
{
    public interface IUserUpdateService
    {
        IActionResult UpdateUser(SidString sidString, IEditOperation operation, ConnectedResponse continuation, in DomainQuery target);
    }

    [DependencyRegistration(typeof(IUserUpdateService), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class UserUpdateService : IUserUpdateService
    {
        private readonly IRequestService _requestSvc;
        private readonly ISchemaService _schemaSvc;

        public UserUpdateService(IRequestService requestSvc, ISchemaService schemaSvc)
        {
            _requestSvc = requestSvc;
            _schemaSvc = schemaSvc;
        }

        public IActionResult UpdateUser(SidString sidString, IEditOperation operation, ConnectedResponse continuation, in DomainQuery target)
        {
            if (string.IsNullOrWhiteSpace(continuation.FoundObject))
            {
                return new ApiBadRequestResult("The distinguished name of the object to update was not found.", ResultCode.NoSuchObject);
            }

            ModifyRequest request = new(continuation.FoundObject);
            operation.ApplyToRequest(request);

            var oneOf = _requestSvc.SendForResponse<ModifyResponse>(request, continuation.ActiveConnection);
            if (oneOf.TryGetT1(out var error, out var answer) || answer.ResultCode != ResultCode.Success)
            {
                return error ?? new ApiBadRequestResult("The update was not successful however no error was generated.", ResultCode.OperationsError);
            }

            return new AcceptedResult();
        }
    }
}

