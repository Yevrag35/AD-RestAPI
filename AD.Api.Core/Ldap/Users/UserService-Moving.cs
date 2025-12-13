using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Security;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Core.Ldap.Users;

public partial interface IUserService
{
	IActionResult Move(SidString userSid, UserMoveRequest request, in DomainQuery target);
}

internal sealed partial class UserService
{
	public IActionResult Move(SidString userSid, UserMoveRequest request, in DomainQuery target)
	{
		var oneOf = this.FindOneAndContinue(userSid, in target);
		if (oneOf.TryGetT1(out IActionResult? error, out ConnectedResponse? continuation))
		{
			return error;
		}

		return _moveSvc.MoveObject(request.NewParentDn.Value, request.NewName.Value, continuation);
	}
}
