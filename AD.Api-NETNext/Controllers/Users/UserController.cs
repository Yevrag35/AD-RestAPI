using AD.Api.Authentication;
using AD.Api.Binding.Attributes;
using AD.Api.Core;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Passwords;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Ldap.Users;
using AD.Api.Core.Security;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.DirectoryServices.Protocols;

namespace AD.Api.Controllers.Users;

[Route(ROUTE_NAME)]
[ApiController]
[Authorize]
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
        [FromServices] IPasswordChangeService pwdSvc,
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

    [HttpPatch]
    [Route("{sid:objectsid}")]
    [JwtAuth(AuthorizedRole.UserEditor, possiblyScoped: true)]
    public IActionResult UpdateUser(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] object body,
        [FromServices] IAuthorizer authorizer,
        [FromRouteSid] SidString sid,
        [Domain] DomainQuery target)
    {
        //if (!authSvc.IsAuthorized(this.HttpContext, request.Path))
        //{
        //    return new ForbidResult();
        //}

        var oneOf = this.UserSearcher.GetOneUserAndContinue(sid, in target);
        if (oneOf.TryGetT1(out IActionResult? error, out ConnectedResponse? continueWith))
        {
            return error;
        }
        
        if (!continueWith.TryGetResponse(out SearchResponse? searchResult) || searchResult.Entries.Count == 0)
        {
            return new NotFoundResult();
        }

        return this.Ok(new
        {
            dn = searchResult.Entries[0].Attributes[AttributeConstants.DISTINGUISHED_NAME].GetValues(typeof(string))[0],
        });
        
    }
}