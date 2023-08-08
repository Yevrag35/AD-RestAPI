using AD.Api.Ldap.Models;
using AD.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Move
{
    [ApiController]
    [Produces("application/json")]
    public class ADMoveController : ADControllerBase
    {
        private IMoveService MoveService { get; }

        public ADMoveController(IIdentityService identityService, IMoveService moveService)
            : base(identityService)
        {
            this.MoveService = moveService;
        }

        [HttpPost]
        [Route("move")]
        public IActionResult MoveObject(
            [FromBody] MoveRequest moveRequest,
            [FromQuery] string? domain = null)
        {
            moveRequest.ClaimsPrincipal = this.HttpContext.User;
            moveRequest.Domain = domain;

            var result = this.MoveService.MoveObject(moveRequest);

            return result.Success
                ? this.Ok(result)
                : this.BadRequest(result);
        }
    }
}
