using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AD.Api.Controllers.Create
{
    [ApiController]
    [Produces("application/json")]
    public class GroupCreateController : ADCreateController
    {
        private IConnectionService Connections { get; }
        private ISchemaService Schema { get; }

        public GroupCreateController(IIdentityService identityService, ICreateService createService, IResultService resultService,
            ISchemaService schemaService, IConnectionService connectionService)
            : base(identityService, createService, resultService)
        {
            this.Connections = connectionService;
            this.Schema = schemaService;
        }

        [HttpPost]
        [Route("create/group")]
        public IActionResult CreateGroup(
            [FromBody] GroupCreateOperationRequest request,
            [FromQuery] string? domain = null)
        {
            request.Domain = domain;
            request.ClaimsPrincipal = this.HttpContext.User;
            RemoveProperties(request, "objectClass", "objectCategory");

            using (var connection = this.Connections.GetConnection(options =>
            {
                options.Domain = request.Domain;
                options.Principal = request.ClaimsPrincipal;
                options.DontDisposeHandle = true;
            }))
            {
                if (!this.Schema.HasAllAttributesCached(request.Properties.Keys, out List<string>? missing))
                {
                    this.Schema.LoadAttributes(missing, connection);
                }
            }

            ISuccessResult createResult = this.CreateService.Create(request);

            return createResult.Success
                ? StatusCode((int)HttpStatusCode.Created, createResult)
                : BadRequest(createResult);
        }
    }
}
