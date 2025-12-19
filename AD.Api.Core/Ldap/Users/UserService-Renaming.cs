using AD.Api.Core.Ldap.Requests;
using AD.Api.Core.Security;

namespace AD.Api.Core.Ldap.Users;

public partial interface IUserService
{
	IActionResult Rename(SidString userSid, RenameRequest request, in DomainQuery target);
}

internal sealed partial class UserService
{
	public IActionResult Rename(SidString userSid, RenameRequest request, in DomainQuery target)
	{
		RelativeName rdn = RelativeName.Create(request.Name, RelativeNameType.CommonName);

		var oneOf = this.FindOneAndContinue(userSid, in target);

		return oneOf.Match(
			(rdn, renameSvc: _renameSvc),
			(state, continuation) => state.renameSvc.RenameObject(state.rdn, continuation),
			(_, error) => error);
	}
}
