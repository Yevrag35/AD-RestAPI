using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Core.Ldap;

public interface IMoveService
{
    IActionResult MoveObject(DistinguishedName newParentDn, RelativeName? newName, ConnectedResponse continuation);
}

[DependencyRegistration(typeof(IMoveService), Lifetime = ServiceLifetime.Singleton)]
internal sealed class MoveService : IMoveService
{
    private readonly IRequestService _requestSvc;

    public MoveService(IRequestService requestSvc)
    {
        _requestSvc = requestSvc;
    }

    public IActionResult MoveObject(DistinguishedName newParentDn, RelativeName? newName, ConnectedResponse continuation)
    {

        string newRdn = newName?.Value ?? continuation.FoundObject[0].Value;
        ModifyDNRequest modify = new((string)continuation.FoundObject, (string)newParentDn, newRdn);

        var oneOf = _requestSvc.SendForResponse<ModifyDNResponse>(modify, continuation.ActiveConnection);
        if (oneOf.TryGetT1(out var error, out var answer) || answer.ResultCode != ResultCode.Success)
        {
            return error ?? new ApiBadRequestResult("The move was not successful however no error was generated.", ResultCode.OperationsError);
        }

        return new AcceptedResult();
    }
}
