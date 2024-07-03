using AD.Api.Authentication;
using AD.Api.Binding.Attributes;
using AD.Api.Core;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Passwords;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Users
{
    [Route("users/passwords")]
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
        [Route("reset")]
        [JwtAuth(AuthorizedRole.PasswordResetter, possiblyScoped: true)]
        public IActionResult ResetPassword(
            [FromBody] PasswordResetRequest request,
            [Domain] DomainQuery target)
        {
            DistinguishedName dn = DistinguishedName.Parse(request.DistinguishedName);

            if (!_authorizer.IsAuthorized(this.HttpContext, dn.Path))
            {
                return new ForbidResult();
            }

            return _resetSvc.Reset(in target, request);
        }
    }
}
