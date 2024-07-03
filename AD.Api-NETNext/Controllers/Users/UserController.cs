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

namespace AD.Api.Controllers.Users;

[Route(ROUTE_NAME)]
[ApiController]
public class UserController : ControllerBase
{
    private const string ROUTE_NAME = "users";
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

    private const string SID_ROUTE_PREFIX = "/" + ROUTE_NAME + "/";
    [HttpPost]
    [JwtAuth(AuthorizedRole.UserCreator, possiblyScoped: true)]
    public IActionResult CreateUser(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] CreateUserRequest request,
        [FromServices] IUserCreations createSvc,
        [FromServices] IAuthorizer authSvc,
        [Domain] DomainQuery target)
    {
        if (!authSvc.IsAuthorized(this.HttpContext, request.Path))
        {
            return new ForbidResult();
        }

        return createSvc.Create(in target, request, SID_ROUTE_PREFIX);
    }
}