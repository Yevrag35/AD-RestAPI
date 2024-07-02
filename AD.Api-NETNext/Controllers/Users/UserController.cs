using AD.Api.Attributes;
using AD.Api.Authentication;
using AD.Api.Binding.Attributes;
using AD.Api.Components;
using AD.Api.Core;
using AD.Api.Core.Authentication;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Users;
using AD.Api.Core.Security;
using AD.Api.Spans;
using AD.Api.Statics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.Api.Controllers.Users
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public IUserSearcher UserSearcher { get; }

        public UserController(IUserSearcher searcher)
        {
            this.UserSearcher = searcher;
        }

        [HttpGet]
        [Route("{sid:objectsid}")]
        [JwtAuth(AuthorizedRole.Reader)]
        public IActionResult GetUser(
            [FromQuery] SearchParameters parameters,
            [FromRouteSid] SidString sid)
        {
            return this.UserSearcher.GetOneUser(sid, parameters, this.HttpContext.RequestServices);
        }

        private const string SID_ROUTE_PREFIX = "/users/";
        [HttpPost]
        [JwtAuth(AuthorizedRole.UserCreator, possiblyScoped: true)]
        public IActionResult CreateUser(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] CreateUserRequest request,
            [FromServices] IUserCreations createSvc,
            [FromServices] IAuthorizer authSvc,
            [Domain] DomainQuery info)
        {
            if (!authSvc.IsAuthorized(this.HttpContext, request.Path))
            {
                return new ForbidResult();
            }

            return createSvc.Create(info.Domain, request, this.HttpContext.RequestServices, info.DomainController)
                .Match(
                    state: (request, info),
                    (request, success) =>
                    {
                        int length = request.info.UrlQueryLength + success.Value.Length + SID_ROUTE_PREFIX.Length + 1;
                        Span<char> chars = stackalloc char[length];
                        int pos = 0;

                        SID_ROUTE_PREFIX.CopyToSlice(chars, ref pos);
                        success.Value.CopyToSlice(chars, ref pos);
                        if (request.info != DomainQuery.Default)
                        {
                            chars[pos++] = CharConstants.QUESTION;
                            request.info.AppendAsQuery(chars.Slice(pos), out int written);
                            pos += written;
                        }

                        return new CreatedResult(new string(chars.Slice(0, pos)), new
                        {
                            Domain = request.info,
                            Dn = request.request.GetDistinguishedName().ToString(),
                            ObjectSid = success.Value,
                        });
                    },
                    (request, error) => error);
        }
    }
}
