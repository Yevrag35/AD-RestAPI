using AD.Api.Authentication;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Search
{
    [ApiController]
    [Authorize]
    public sealed class SearchController : ControllerBase
    {
        public ILdapFilterService Filters { get; }
        public IRequestService Requests { get; }

        public SearchController(ILdapFilterService filterSvc, IRequestService requests)
        {
            this.Filters = filterSvc;
            this.Requests = requests;
        }

        [HttpPost]
        [Route("search")]
        [JwtAuth(AuthorizedRole.Reader)]
        [ProducesResponseType(200, Type = typeof(CollectionResponse))]
        [ProducesResponseType(400, Type = typeof(ApiBadRequestResult))]
        public IActionResult SearchObjects(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters)
        {
            parameters.ApplyParameters(body);
            return this.Requests.FindAll(parameters, this.HttpContext.RequestServices);
        }

        //[HttpGet]
        //public IActionResult ContinueSearch(
        //    [FromQuery] Guid continueKey,
        //    [FromServices] CollectionResponse response,
        //    [FromServices] ISearchPagingService pagingSvc,
        //    [FromServices] IPooledItem<ResultEntryCollection> results)
        //{
        //}

        [HttpPost]
        [Route("computers/search")]
        [JwtAuth(AuthorizedRole.Reader)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult SearchComputers(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters)
        {
            body.Filter = this.Filters.AddToFilter(body.Filter, FilteredRequestType.Computer, true);
            body.RequestBaseType = FilteredRequestType.Computer;

            parameters.ApplyParameters(body);
            return this.Requests.FindAll(parameters, this.HttpContext.RequestServices);
        }

        [HttpPost]
        [Route("groups/search")]
        [JwtAuth(AuthorizedRole.Reader)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult SearchGroups(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters)
        {
            body.Filter = this.Filters.AddToFilter(body.Filter, FilteredRequestType.Group, true);
            body.RequestBaseType = FilteredRequestType.Group;

            parameters.ApplyParameters(body);
            return this.Requests.FindAll(parameters, this.HttpContext.RequestServices);
        }

        [HttpPost]
        [Route("users/search")]
        [JwtAuth(AuthorizedRole.Reader)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult SearchUsers(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters)
        {
            body.RequestBaseType = FilteredRequestType.User;

            body.Filter = this.Filters.AddToFilter(body.Filter, FilteredRequestType.User, true);
            parameters.ApplyParameters(body);
            return this.Requests.FindAll(parameters, this.HttpContext.RequestServices);
        }
    }
}
