using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Serialization;
using AD.Api.Pooling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.DirectoryServices.Protocols;

namespace AD.Api.Controllers.Search
{
    [Route("search")]
    [ApiController]
    public sealed class SearchController : ControllerBase
    {
        public IConnectionService Connections { get; }

        public SearchController(IConnectionService connectSvc)
        {
            this.Connections = connectSvc;
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult SearchObjects(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters,
            [FromServices] CollectionResponse response,
            [FromServices] IPooledItem<ResultEntryCollection> results)
        {
            results.Value.Domain = parameters.Domain ?? string.Empty;

            LdapConnection connection = parameters.ApplyConnection(this.Connections);

            parameters.ApplyParameters(body);

            SearchResponse searchResponse;
            try
            {
                searchResponse = (SearchResponse)connection.SendRequest(parameters);
            }
            catch (DirectoryException ex)
            {
                response.Error = ex.Message;
                response.AddResultCode = true;
                if (ex is LdapException ldapEx)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.ErrorCode = ldapEx.ErrorCode;
                    response.Result = ResultCode.OperationsError;
                }
                else if (ex is DirectoryOperationException dirEx)
                {
                    response.Result = dirEx.Response.ResultCode;
                }

                return response;
            }

            if (searchResponse.ResultCode != ResultCode.Success)
            {
                return this.BadRequest(new
                {
                    Result = searchResponse.ResultCode,
                    ResultCode = (int)searchResponse.ResultCode,
                    Message = searchResponse.ErrorMessage ?? "No entries were found.",
                });
            }
            else if (searchResponse.Entries.Count <= 0)
            {
                return this.Ok(new
                {
                    Result = searchResponse.ResultCode
                });
            }

            results.Value.AddRange(searchResponse.Entries);
            response.SetData(searchResponse, results.Value);
            return response;
        }

        //[HttpPost]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(400)]
        //[Route("")]

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("users")]
        public IActionResult SearchUsers(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters,
            [FromServices] CollectionResponse response,
            [FromServices] IPooledItem<ResultEntryCollection> results)
        {
            results.Value.Domain = parameters.Domain ?? string.Empty;
            body.LdapFilter = body.LdapFilter[0] != '(' && body.LdapFilter[^1] != ')'
                ? $"({body.LdapFilter})"
                : body.LdapFilter;

            body.LdapFilter = $"(&(objectClass=user)(objectCategory=person){body.LdapFilter})";
            LdapConnection connection = parameters.ApplyConnection(this.Connections);
            parameters.ApplyParameters(body);

            var searchResponse = (SearchResponse)connection.SendRequest(parameters);
            if (searchResponse.ResultCode != ResultCode.Success)
            {
                return this.BadRequest(new
                {
                    Result = searchResponse.ResultCode,
                    ResultCode = (int)searchResponse.ResultCode,
                    Message = searchResponse.ErrorMessage ?? "No entries were found.",
                });
            }

            results.Value.AddRange(searchResponse.Entries);
            response.AddResultCode = true;
            response.Result = searchResponse.ResultCode;
            response.SetData(results.Value, results.Value.Count);

            return response;
        }
    }
}
