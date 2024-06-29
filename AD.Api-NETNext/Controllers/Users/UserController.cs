using AD.Api.Authentication;
using AD.Api.Binding.Attributes;
using AD.Api.Core.Authentication;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Users;
using AD.Api.Core.Security;
using AD.Api.Core.Web.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.Api.Controllers.Users
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public IRequestService Requests { get; }

        public UserController(IRequestService requestSvc)
        {
            this.Requests = requestSvc;
        }

        [HttpGet]
        [Route("{sid:objectsid}")]
        [AuthenticatedUser(AuthorizedRole.Reader)]
        public IActionResult GetUser(
            [FromQuery] SearchParameters parameters,
            [FromRouteSid] SidString sid)
        {
            string filter = parameters.FilterSvc.GetFilter(sid, FilteredRequestType.User);

            SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.User);
            parameters.ApplyParameters(searchFilter);

            return this.Requests.FindOne(parameters, this.HttpContext.RequestServices);
        }

        [HttpPost]
        [AuthenticatedUser(AuthorizedRole.UserCreator, possibleScoped: true)]
        public IActionResult CreateUser(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] CreateUserRequest request,
            [FromServices] IUserCreations createSvc,
            [FromServices] IAuthorizationService authSvc,
            [QueryDomain] string? domain = null,
            [FromQuery] string? dc = null)
        {
            if (!authSvc.IsAuthorized(this.HttpContext, request.Path))
            {
                return new ForbidResult();
            }

            request.SetRequestServices(this.HttpContext);
            return createSvc.Create(domain, request, dc)
                .Match(
                    state: (request, domain),
                    (request, success) =>
                    {
                        string url = !string.IsNullOrWhiteSpace(request.domain)
                            ? $"/users/{success.Value}?domain={request.domain}"
                            : $"/users/{success.Value}";

                        return new CreatedResult(url, new
                        {
                            Domain = request.domain ?? string.Empty,
                            Dn = request.request.GetDistinguishedName().ToString(),
                            ObjectSid = success.Value,
                        });
                    },
                    (request, error) => error);
        }
    }
}
