using AD.Api.Core.Ldap;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Search
{
    [Route("search")]
    [ApiController]
    public sealed class SearchController : ControllerBase
    {
        public IRequestService Requests { get; }

        public SearchController(IRequestService requests)
        {
            this.Requests = requests;
        }

        [HttpPost]
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
        //    if (!pagingSvc.TryGetRequest(continueKey, this.HttpContext, out CachedSearchParameters? parameters))
        //    {
        //        return this.BadRequest(new
        //        {
        //            Message = "Bad continue key",
        //        });
        //    }

        //    return this.SearchObjects(null!, parameters, response, results);
        //}

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("users")]
        public IActionResult SearchUsers(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters)
        {
            body.Filter = body.Filter[0] != '(' && body.Filter[^1] != ')'
                ? $"({body.Filter})"
                : body.Filter;

            body.Filter = $"(&(objectClass=user)(objectCategory=person){body.Filter})";
            parameters.ApplyParameters(body);
            return this.Requests.FindAll(parameters, this.HttpContext.RequestServices);
            //LdapConnection connection = parameters.ApplyConnection(this.Connections);

            //var searchResponse = (SearchResponse)connection.SendRequest(parameters);
            //if (searchResponse.ResultCode != ResultCode.Success)
            //{
            //    return this.BadRequest(new
            //    {
            //        Result = searchResponse.ResultCode,
            //        ResultCode = (int)searchResponse.ResultCode,
            //        Message = searchResponse.ErrorMessage ?? "No entries were found.",
            //    });
            //}

            //results.Value.AddRange(searchResponse.Entries);
            //response.AddResultCode = true;
            //response.Result = searchResponse.ResultCode;
            //response.SetData(results.Value, results.Value.Count);

            //return response;
        }
    }
}
