using AD.Api.Binding.Attributes;
using AD.Api.Core.Ldap.Requests.Search;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Security;
using AD.Api.Pooling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;
using System.DirectoryServices.Protocols;

namespace AD.Api.Controllers.Users
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public IConnectionService Connections { get; }

        public UserController(IConnectionService connectionService)
        {
            this.Connections = connectionService;
        }

        [HttpGet]
        [Route("{sid}")]
        public IActionResult GetUser(
            [FromServices] IPooledItem<ResultEntry> result,
            [FromQuery] SearchParameters parameters,
            [FromRouteSid] SidString sid)
        {
            LdapConnection connection = parameters.ApplyConnection(this.Connections);
            string filter = sid.ToLdapString();
            parameters.ApplyParameters($"(objectSid={filter})");

            var response = (SearchResponse)connection.SendRequest(parameters);
            if (response.ResultCode != ResultCode.Success || response.Entries.Count > 1)
            {
                return this.BadRequest(new
                {
                    Result = response.ResultCode,
                    ResultCode = (int)response.ResultCode,
                    Message = response.ErrorMessage ?? "More than one entry was found.",
                });
            }

            result.Value.AddResult(parameters.Domain, response.Entries[0]);
            return this.Ok(result.Value);
        }
    }
}
