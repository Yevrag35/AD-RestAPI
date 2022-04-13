using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Schema;
using AD.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace AD.Api.Controllers
{
    [Route("create/user")]
    [ApiController]
    [Produces("application/json")]
    public class UserCreateController : ADCreateController
    {
        private IConnectionService Connections { get; }
        private ISchemaService Schema { get; }

        public UserCreateController(ICreateService createService, ISchemaService schemaService, IConnectionService connectionService)
            : base(createService)
        {
            this.Connections = connectionService;
            this.Schema = schemaService;
        }

        [HttpPost]
        public IActionResult TestSendBack([FromBody] UserCreateOperationRequest request)
        {
            RemoveProperties(request, "objectClass", "objectCategory");

            using (var connection = this.Connections.GetConnection(request.Domain))
            {
                if (!this.Schema.HasAllAttributesCached(request.Properties.Keys, out List<string>? missing))
                {
                    this.Schema.LoadAttributes(missing, connection);
                }
            }

            if (this.Schema.Dictionary.TryGetValue("userAccountControl", out SchemaProperty? prop))
            {
                return Ok(prop);
            }
            else
            {
                return NotFound(":(");
            }

            //var result = this.CreateService.Create(request);

            //return Ok(result);
        }
    }
}
