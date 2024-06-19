using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Results;
using AD.Api.Pooling;
using Microsoft.AspNetCore.Mvc;
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
            [FromServices] IPooledItem<ResultEntryCollection> results)
        {
            results.Value.Domain = parameters.Domain ?? string.Empty;

            LdapConnection connection = parameters.ApplyConnection(this.Connections);
            parameters.ApplyParameters(body.LdapFilter);

            var response = (SearchResponse)connection.SendRequest(parameters);
            if (response.ResultCode != ResultCode.Success)
            {
                return this.BadRequest(new
                {
                    Result = response.ResultCode,
                    ResultCode = (int)response.ResultCode,
                    Message = response.ErrorMessage ?? "No entries were found.",
                });
            }

            results.Value.AddRange(response.Entries);
            return this.Ok(new
            {
                Count = results.Value.Count,
                Data = results.Value,
            });
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("users")]
        public IActionResult SearchUsers(
            [FromBody] SearchFilterBody body,
            [FromQuery] SearchParameters parameters,
            [FromServices] IPooledItem<ResultEntryCollection> results)
        {
            results.Value.Domain = parameters.Domain ?? string.Empty;
            body.LdapFilter = body.LdapFilter[0] != '(' && body.LdapFilter[^1] != ')'
                ? $"({body.LdapFilter})"
                : body.LdapFilter;

            body.LdapFilter = $"(&(objectClass=user)(objectCategory=person){body.LdapFilter})";
            LdapConnection connection = parameters.ApplyConnection(this.Connections);
            parameters.ApplyParameters(body.LdapFilter);

            var response = (SearchResponse)connection.SendRequest(parameters);
            if (response.ResultCode != ResultCode.Success)
            {
                return this.BadRequest(new
                {
                    Result = response.ResultCode,
                    ResultCode = (int)response.ResultCode,
                    Message = response.ErrorMessage ?? "No entries were found.",
                });
            }

            results.Value.AddRange(response.Entries);
            return this.Ok(new
            {
                Count = results.Value.Count,
                Data = results.Value,
            });
        }
    }
}
