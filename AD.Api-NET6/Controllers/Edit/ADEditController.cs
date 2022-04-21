using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using AD.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32.SafeHandles;
using System.DirectoryServices;
using System.Net;
using System.Security.Principal;

namespace AD.Api.Controllers.Edit
{
    [ApiController]
    [Produces("application/json")]
    public class ADEditController : ADControllerBase
    {
        private IConnectionService Connections { get; }
        private IEditService EditService { get; }
        private ISchemaService Schema { get; }

        public ADEditController(IIdentityService identityService, IConnectionService conService, IEditService editService,
            ISchemaService schemaService)
            : base(identityService)
        {
            this.Connections = conService;
            this.EditService = editService;
            this.Schema = schemaService;
        }

        /// <summary>
        /// Edits an account.
        /// </summary>
        /// <param name="request">The request body containing the DistinguishedName (DN)
        /// of the object to modify with the specified edit operations.</param>
        /// <param name="domain">The domain the of the object being edited.</param>
        /// <response code="202">The server successfully applied the modifications.</response>
        /// <response code="400">The JSON request body is malformed.</response>
        /// <response code="422">The encountered an error attempting to apply the modifications.</response>
        [HttpPut]
        [Route("edit")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(ISuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(IErroredResult))]
        public IActionResult PerformEdit(
            [FromBody] EditOperationRequest request,
            [FromQuery] string? domain = null)
        {
            request.Domain = domain;
            request.ClaimsPrincipal = this.HttpContext.User;

            SafeAccessTokenHandle? token = ((WindowsIdentity?)this.HttpContext?.User?.Identity)?.AccessToken;

            using (var connection = this.Connections.GetConnection(options =>
            {
                options.Domain = request.Domain;
                options.DontDisposeHandle = false;
                options.Principal = this.HttpContext?.User;
            }))
            {
                if (!this.Schema.HasAllAttributesCached(request.EditOperations.Select(x => x.Property), out List<string>? missing))
                {
                    this.Schema.LoadAttributes(missing, connection);
                }

                using DirectoryEntry de = connection.GetDirectoryEntry(request.DistinguishedName);

                ISuccessResult editResult = this.EditService.Edit(de, request.EditOperations, token);

                return editResult.Success
                ? Accepted(editResult)
                : UnprocessableEntity(editResult);
            }
        }
    }
}
