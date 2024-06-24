using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Search
{
    [ApiController]
    public sealed class SearchController : ControllerBase
    {
        public IRequestService Requests { get; }

        public SearchController(IRequestService requests)
        {
            this.Requests = requests;
        }

        [HttpPost]
        [Route("search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
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
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult SearchComputers(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters)
        {
            body.Filter = parameters.FilterSvc.AddToFilter(body.Filter, FilteredRequestType.Computer, true);
            body.RequestBaseType = FilteredRequestType.Computer;

            parameters.ApplyParameters(body);
            return this.Requests.FindAll(parameters, this.HttpContext.RequestServices);
        }

        [HttpPost]
        [Route("groups/search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult SearchGroups(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters)
        {
            body.Filter = parameters.FilterSvc.AddToFilter(body.Filter, FilteredRequestType.Group, true);
            body.RequestBaseType = FilteredRequestType.Group;

            parameters.ApplyParameters(body);
            return this.Requests.FindAll(parameters, this.HttpContext.RequestServices);
        }

        [HttpPost]
        [Route("users/search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult SearchUsers(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters)
        {
            body.Filter = parameters.FilterSvc.AddToFilter(body.Filter, FilteredRequestType.User, true);
            body.RequestBaseType = FilteredRequestType.User;

            parameters.ApplyParameters(body);
            return this.Requests.FindAll(parameters, this.HttpContext.RequestServices);
        }
    }
}
