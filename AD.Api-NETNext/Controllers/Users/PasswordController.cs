using AD.Api.Attributes;
using AD.Api.Authentication;
using AD.Api.Binding.Attributes;
using AD.Api.Core;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Passwords;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Users;

[Authorize]
[ApiController]
[LdapRequiresSSL]
[Route("passwords")]
public sealed class PasswordController : ControllerBase
{
    private readonly IPasswordChangeService _changeSvc;
    private readonly IAuthorizer _authorizer;
    private readonly IPasswordResetService _resetSvc;

    public PasswordController(IAuthorizer authorizer, IPasswordChangeService changeSvc, IPasswordResetService resetSvc)
    {
        _authorizer = authorizer;
        _changeSvc = changeSvc;
        _resetSvc = resetSvc;
    }

    [HttpPut]
    [Route("change")]
    [JwtAuth(AuthorizedRole.PasswordChanger, PossiblyScoped = true)]
    public IActionResult ChangePassword(
        [FromBody] PasswordChangeRequestByDN request,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        if (!_authorizer.IsAuthorized(this.HttpContext, request.DistinguishedName, out var role))
        {
            return new ForbidResult();
        }

        return _changeSvc.Change(in target, request);
    }

    [HttpPut]
    [Route("reset")]
    [JwtAuth(AuthorizedRole.PasswordResetter, PossiblyScoped = true)]
    public IActionResult ResetPassword(
        [FromBody] PasswordResetRequestByDN request,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        if (!_authorizer.IsAuthorized(this.HttpContext, request.DistinguishedName, out _))
        {
            return new ForbidResult();
        }

        return _resetSvc.Reset(in target, request);
    }
}
