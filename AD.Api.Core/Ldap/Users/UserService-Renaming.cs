using AD.Api.Core.Ldap.Requests;
using AD.Api.Core.Security;
using Microsoft.AspNetCore.Mvc;

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
            state: (rdn, renameSvc: _renameSvc),
            f0: (state, continuation) => state.renameSvc.RenameObject(state.rdn, continuation),
            f1: (_, error) => error);
    }
}
