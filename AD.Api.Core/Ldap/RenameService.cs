using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Core.Ldap;

public interface IRenameService
{
    IActionResult RenameObject(RelativeName newName, ConnectedResponse continuation);
}

[DependencyRegistration(typeof(IRenameService), Lifetime = ServiceLifetime.Singleton)]
internal sealed class RenameService : IRenameService
{
    private readonly IRequestService _requestSvc;

    public RenameService(IRequestService requestSvc)
    {
        _requestSvc = requestSvc;
    }

    public IActionResult RenameObject(RelativeName newName, ConnectedResponse continuation)
    {
        if (newName.IsEmpty || newName.AttributeType == RelativeNameType.DomainComponent)
        {
            throw new ArgumentException("The new name must be non-empty and not a domain component.");
        }

        DistinguishedName currentDn = continuation.FoundObject;
        ModifyDNRequest modify = new((string)currentDn, null, newName.Value);

        var oneOf = _requestSvc.SendForResponse<ModifyDNResponse>(modify, continuation.ActiveConnection);
        if (oneOf.TryGetT1(out var error, out var answer) || answer.ResultCode != ResultCode.Success)
        {
            return error ?? new ApiBadRequestResult("The rename was not successful however no error was generated.", ResultCode.OperationsError);
        }

        return new AcceptedResult();
    }
}
