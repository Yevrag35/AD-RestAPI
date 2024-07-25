using AD.Api.Attributes;
using AD.Api.Authentication;
using AD.Api.Binding.Attributes;
using AD.Api.Core;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Groups;
using AD.Api.Core.Ldap.Passwords;
using AD.Api.Core.Ldap.Requests;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Ldap.Users;
using AD.Api.Core.Operations;
using AD.Api.Core.Security;
using AD.Api.Core.Web;
using AD.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NLog;
using System.Security.Principal;

namespace AD.Api.Controllers.Users;

[ApiAuthorize]
[ApiController]
[Route(ROUTE_NAME)]
public sealed class UsersController : ControllerBase
{
    private const string ROUTE_NAME = "users";
    static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public IUserSearcher UserSearcher { get; }

    public UsersController(IUserSearcher searcher)
    {
        this.UserSearcher = searcher;
    }

    [HttpGet]
    [Route("{sid:objectsid}")]
    [JwtAuth(AuthorizedRole.Reader)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateErrorBody))]
    public IActionResult GetUser(
        [FromQuery] SearchParameters parameters,
        [FromServices] IPasswordChangeService pwdSvc,
        [FromRouteSid] SidString sid)
    {
        SecurityIdentifier id = new();
        return this.UserSearcher.FindOne(sid, parameters, this.HttpContext.RequestServices);
    }

    private const string SID_ROUTE_PREFIX = "/" + ROUTE_NAME + "/";
    [HttpPost]
    [JwtAuth(AuthorizedRole.UserCreator, PossiblyScoped = true)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreatedResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateErrorBody))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult CreateUser(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] CreateUserRequest request,
        [FromServices] IUserCreations createSvc,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
           return new ApiBadRequestResult(this.ModelState);
        }

        return createSvc.Create(in target, request, SID_ROUTE_PREFIX);
    }

    [HttpDelete]
    [Route("{sid:objectsid}")]
    [JwtAuth(AuthorizedRole.UserDeleter, PossiblyScoped = true)]
    public IActionResult DeleteUser(
        [FromRouteSid] SidString sid,
        [FromServices] IDeletionService deleteSvc,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        var userSearch = this.UserSearcher.FindOneAndContinue(sid, in target);
        if (userSearch.TryGetT1(out var error, out var continuation))
        {
            return error;
        }

        return deleteSvc.DeleteObject(continuation);
    }

    private static readonly string[] _uac = [AttributeConstants.DISTINGUISHED_NAME, AttributeConstants.USER_ACCOUNT_CONTROL];
    [HttpPut]
    [Route("{sid:objectsid}/disable")]
    [JwtAuth(AuthorizedRole.UserAdmin, PossiblyScoped = true)]
    public IActionResult DisableUser(
        [FromRouteSid] SidString sid,
        [FromServices] IUserUpdateService updateSvc,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        var userSearch = this.UserSearcher.FindOneAndContinue(sid, in target, _uac);
        if (userSearch.TryGetT1(out var error, out var continuation))
        {
            return error;
        }

        return updateSvc.ToggleStatus(sid, new AccountStatusUpdateRequest(false), continuation, in target);
    }
    [HttpPut]
    [Route("{sid:objectsid}/enable")]
    [JwtAuth(AuthorizedRole.UserAdmin, PossiblyScoped = true)]
    public IActionResult EnableUser(
        [FromRouteSid] SidString sid,
        [FromServices] IUserUpdateService updateSvc,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        var userSearch = this.UserSearcher.FindOneAndContinue(sid, in target, _uac);
        if (userSearch.TryGetT1(out var error, out var continuation))
        {
            return error;
        }

        return updateSvc.ToggleStatus(sid, new AccountStatusUpdateRequest(true), continuation, in target);
    }

    private static readonly string[] _groupProperties = [AttributeConstants.DISTINGUISHED_NAME, AttributeConstants.MEMBER_OF];
    [HttpGet]
    [Route("{sid:objectsid}/groups")]
    [JwtAuth(AuthorizedRole.Reader)]
    public IActionResult GetUserGroups(
        [FromRouteSid] SidString sid,
        [FromServices] IGroupSearcher groupSearcher,
        [Domain] DomainQuery target,
        [FromQuery] string? properties = null)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        var userSearch = this.UserSearcher.FindOneAndContinue(sid, in target, _groupProperties);
        if (userSearch.TryGetT1(out var error, out var continuation))
        {
            return error;
        }

        return groupSearcher.ResolveUserGroups(continuation, properties);
    }

    [HttpPut]
    [Route("{sid:objectsid}/move")]
    [JwtAuth(AuthorizedRole.UserEditor, PossiblyScoped = true)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateErrorBody))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult MoveUser(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] UserMoveRequest request,
        [FromRouteSid] SidString sid,
        [FromServices] IMoveService moveSvc,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        var userSearch = this.UserSearcher.FindOneAndContinue(sid, in target);
        if (userSearch.TryGetT1(out var error, out var continuation))
        {
            return error;
        }

        return moveSvc.MoveObject(request.NewParentDn.Value, request.NewName.Value, continuation);
    }

    [HttpPut]
    [Route("{sid:objectsid}/rename")]
    [JwtAuth(AuthorizedRole.UserEditor, PossiblyScoped = true)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateErrorBody))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult RenameUser(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] RenameRequest request,
        [FromRouteSid] SidString sid,
        [FromServices] IRenameService renameSvc,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        RelativeName rdn = RelativeName.Create(request.Name, RelativeNameType.CommonName);

        var userSearch = this.UserSearcher.FindOneAndContinue(sid, in target);
        if (userSearch.TryGetT1(out var error, out var continuation))
        {
            return error;
        }

        return renameSvc.RenameObject(rdn, continuation);
    }

    [HttpPatch]
    [Route("{sid:objectsid}")]
    [JwtAuth(AuthorizedRole.UserEditor, PossiblyScoped = true)]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(AcceptedResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateErrorBody))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult UpdateUser(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] EditObjectRequest body,
        [FromServices] IUserUpdateService updateSvc,
        [FromRouteSid] SidString sid,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        var oneOf = this.UserSearcher.FindOneAndContinue(sid, in target);
        if (oneOf.TryGetT1(out IActionResult? error, out ConnectedResponse? continueWith))
        {
            return error;
        }

        return updateSvc.UpdateUser(sid, body, continueWith, in target)
                        .WithLocation(sid.Value, ROUTE_NAME, in target);
    }

    // Remove User

    // Change Password
    [HttpPut]
    [LdapRequiresSSL]
    [Route("{sid:objectsid}/password")]
    [JwtAuth(AuthorizedRole.PasswordChanger, PossiblyScoped = true)]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(AcceptedResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateErrorBody))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbidResult))]
    public IActionResult ChangeUserPassword(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] PasswordChangeRequest request,
        [FromServices] IPasswordChangeService pwdChangeSvc,
        [FromRouteSid] SidString sid,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        var oneOf = this.UserSearcher.FindOneAndContinue(sid, in target);
        if (oneOf.TryGetT1(out IActionResult? error, out ConnectedResponse? continueWith))
        {
            return error;
        }

        request.SetContinuation(continueWith);

        return pwdChangeSvc.Change(in target, request)
                           .WithLocation(sid.Value, ROUTE_NAME, in target);
    }

    // Reset Password
    [HttpPut]
    [LdapRequiresSSL]
    [Route("{sid:objectsid}/password/reset")]
    [JwtAuth(AuthorizedRole.PasswordResetter, PossiblyScoped = true)]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(AcceptedResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateErrorBody))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbidResult))]
    public IActionResult ResetUserPassword(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] PasswordResetRequest request,
        [FromServices] IPasswordResetService pwdResetSvc,
        [FromRouteSid] SidString sid,
        [Domain] DomainQuery target)
    {
        if (!this.ModelState.IsValid)
        {
            return new ApiBadRequestResult(this.ModelState);
        }

        var oneOf = this.UserSearcher.FindOneAndContinue(sid, in target);
        if (oneOf.TryGetT1(out IActionResult? error, out ConnectedResponse? continueWith))
        {
            return error;
        }

        request.SetContinuation(continueWith);

        return pwdResetSvc.Reset(in target, request)
                          .WithLocation(sid.Value, ROUTE_NAME, in target);
    }
}