using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Web;

namespace AD.Api.Core.Ldap;

public interface IDeletionService
{
	IActionResult DeleteObject(ConnectedResponse continuation);
}

[DependencyRegistration(typeof(IDeletionService), Lifetime = ServiceLifetime.Singleton)]
internal sealed class DeletionService : IDeletionService
{
	private readonly IRequestService _requestSvc;

	public DeletionService(IRequestService requestSvc)
	{
		_requestSvc = requestSvc;
	}

	public IActionResult DeleteObject(ConnectedResponse continuation)
	{
		DeleteRequest deletion = new(continuation.FoundObject.ToString());

		var oneOf = _requestSvc.SendForResponse<DeleteResponse>(deletion, continuation.ActiveConnection);
		if (oneOf.TryGetT2(out var error, out var answer) || answer.ResultCode != ResultCode.Success)
		{
			return error ?? new ApiBadRequestResult("The delete request was not successful however no error was generated.", ResultCode.OperationsError);
		}

		return new NoContentResult();
	}
}
