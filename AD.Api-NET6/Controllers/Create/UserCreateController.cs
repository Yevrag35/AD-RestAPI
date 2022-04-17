using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Schema;
using AD.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Net;
using System.Security.Principal;

namespace AD.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class UserCreateController : ADCreateController
    {
        private IConnectionService Connections { get; }
        private ISchemaService Schema { get; }

        public UserCreateController(IIdentityService identityService, ICreateService createService, IResultService resultService,
            ISchemaService schemaService, IConnectionService connectionService)
            : base(identityService, createService, resultService)
        {
            this.Connections = connectionService;
            this.Schema = schemaService;
        }

        /// <summary>
        /// Creates a user account.
        /// </summary>
        /// <param name="request">A user request post body including all of the attributes and necessary information.</param>
        /// <param name="domain">The domain to create the user account in.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        /// <example>
        /// POST /create/user
        /// {
        ///     "displayName": "whatev",
        ///     "cn": "asdfasdf",
        ///     "path": "OU=AllMyUsers,DC=contoso,DC=local",
        ///     "mail": "nooneasked@dontcare.net",
        ///     "mailNickname": "asdfasdf",
        ///     "manager": "CN=Some Guy Nobody Likes,OU=Lots Of People Above Them,DC=contoso,DC=local",
        ///     "proxyAddresses": [
        ///         "SMTP:nooneasked@dontcare.net",
        ///         "smtp:someotheraliasnobodyeveremailsthematbuttheysweartheyneedit@useless.com"
        ///     ],
        ///     "userAccountControl": "NormalUser, PasswordNeverExpires",
        ///     "userPrincipalName": "asdfasdf@contoso.local"
        /// }
        /// </example>
        /// <response code="201">A success message is returned.</response>
        /// <response code="400">The request body is malformed.</response>
        /// <response code="422">The LDAP operation failed to commit the changes from the request.  The response will have more error details.</response>
        [HttpPost]
        [Route("create/user")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IDNResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(IErroredResult))]
        public IActionResult CreateUser(
            [FromBody] UserCreateOperationRequest request,
            [FromQuery] string? domain = null
        )
        {
            request.Domain = domain;
            request.ClaimsPrincipal = this.HttpContext.User;
            RemoveProperties(request, "objectClass", "objectCategory");

            using (var connection = this.Connections.GetConnection(new ConnectionOptions
            {
                Domain = request.Domain,
                DontDisposeHandle = true,
                Principal = request.ClaimsPrincipal
            }))
            {
                if (!this.Schema.HasAllAttributesCached(request.Properties.Keys, out List<string>? missing))
                {
                    this.Schema.LoadAttributes(missing, connection);
                }
            }

            OperationResult createResult = this.CreateService.Create(request);

            return createResult.Success
                ? StatusCode((int)HttpStatusCode.Created, createResult)
                : UnprocessableEntity(createResult);
        }
    }
}
