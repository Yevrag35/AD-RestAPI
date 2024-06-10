using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Rename
{
    [ApiController]
    public class ADRenameController : ADControllerBase
    {
        private IRenameService RenameService { get; }

        public ADRenameController(IIdentityService identityService, IRenameService renameService)
            : base(identityService)
        {
            this.RenameService = renameService;
        }

        [HttpPut]
        [Route("rename")]
        public IActionResult RenameObject(
            [FromBody] RenameRequest request,
            [FromQuery] string? domain = null)
        {
            request.Domain = domain;
            request.ClaimsPrincipal = this.HttpContext.User;

            ISuccessResult result = this.RenameService.Rename(request);

            return result.Success
                ? this.Ok(result)
                : this.BadRequest(result);
        }
    }
}
