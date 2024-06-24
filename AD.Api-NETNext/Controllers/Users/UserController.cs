using AD.Api.Binding.Attributes;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Security;
using AD.Api.Pooling;
using AD.Api.Strings.Spans;
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
    }
}
