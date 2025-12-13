using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Requests;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Operations;
using AD.Api.Core.Security;
using AD.Api.Core.Serialization;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Core.Ldap.Users;

public interface IUserUpdateService
{
	IActionResult ToggleStatus(SidString sidString, AccountStatusUpdateRequest operation, ConnectedResponse continuation, in DomainQuery target);
	IActionResult UpdateUser(SidString sidString, IEditOperation operation, ConnectedResponse continuation, in DomainQuery target);
}

[DependencyRegistration(typeof(IUserUpdateService), Lifetime = ServiceLifetime.Singleton)]
internal sealed class UserUpdateService : IUserUpdateService
{
	private readonly IAttributeConverter _converter;
	private readonly IRequestService _requestSvc;

	public UserUpdateService(IRequestService requestSvc, IAttributeConverter converter)
	{
		_requestSvc = requestSvc;
		_converter = converter;
	}

	public IActionResult ToggleStatus(SidString sidString, AccountStatusUpdateRequest operation, ConnectedResponse continuation, in DomainQuery target)
	{
		if (!continuation.IsSearchSuccess)
		{
			return new ApiBadRequestResult("The distinguished name of the object to update was not found.", ResultCode.NoSuchObject);
		}

		ModifyRequest request = new(continuation.FoundObject.ToString());
		operation.ReadChangeFromCurrent(continuation.ResultEntry);
		operation.ApplyToRequest(request);

		var oneOf = _requestSvc.SendForResponse<ModifyResponse>(request, continuation.ActiveConnection);
		if (oneOf.TryGetT1(out var error, out var answer) || answer.ResultCode != ResultCode.Success)
		{
			return error ??
				new ApiBadRequestResult("The update was not successful however no error was generated.", ResultCode.OperationsError);
		}

		return new AcceptedResult($"/users/{sidString.Value}", null);
	}
	public IActionResult UpdateUser(SidString sidString, IEditOperation operation, ConnectedResponse continuation, in DomainQuery target)
	{
		if (continuation.FoundObject.IsEmpty)
		{
			return new ApiBadRequestResult("The distinguished name of the object to update was not found.", ResultCode.NoSuchObject);
		}

		ModifyRequest request = new(continuation.FoundObject.ToString());
		operation.ApplyToRequest(request);

		var oneOf = _requestSvc.SendForResponse<ModifyResponse>(request, continuation.ActiveConnection);
		if (oneOf.TryGetT1(out var error, out var answer) || answer.ResultCode != ResultCode.Success)
		{
			return error ??
				new ApiBadRequestResult("The update was not successful however no error was generated.", ResultCode.OperationsError);
		}

		return new AcceptedResult($"/users/{sidString.Value}", null);
	}
}

