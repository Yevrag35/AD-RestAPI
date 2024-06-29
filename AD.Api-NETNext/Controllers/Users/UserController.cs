using AD.Api.Binding.Attributes;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Users;
using AD.Api.Core.Security;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult CreateUser(
            [FromBody] CreateUserRequest request,
            [FromServices] IUserCreations createSvc,
            [QueryDomain] string? domain = null)
        {
            request.RequestServices = this.HttpContext.RequestServices;
            return createSvc.Create(domain, request)
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
