using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers
{
    [Route("create/user")]
    [ApiController]
    [Produces("application/json")]
    public class UserCreateController : ADCreateController
    {
        public UserCreateController(ICreateService createService)
            : base(createService)
        {
        }

        [HttpPost]
        public IActionResult TestSendBack([FromBody] UserCreateOperationRequest request)
        {
            RemoveProperties(request, "objectClass", "objectCategory");

            var result = this.CreateService.Create(request);

            return Ok(result);
        }
    }
}
