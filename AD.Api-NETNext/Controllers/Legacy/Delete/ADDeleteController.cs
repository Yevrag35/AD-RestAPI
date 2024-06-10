using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Delete
{
    [ApiController]
    [Produces("application/json")]
    public sealed class ADDeleteController : ADControllerBase
    {
        private IDeleteService DeleteService { get; }
        
        public ADDeleteController(IDeleteService deleteService, IIdentityService identityService)
            : base(identityService)
        {
            this.DeleteService = deleteService;
        }

        /// <summary>
        /// Deletes an object.
        /// </summary>
        /// <param name="dn">The distinguishedName of the object to delete.</param>
        /// <param name="domain">The domain where the object to delete resides.</param>
        /// <response code="200">The server successfully deleted the object.</response>
        /// <response code="400">The server was unable to delete the object - most likely due to permissions.</response>
        [HttpDelete]
        [Route("delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ISuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(IErroredResult))]
        public IActionResult DeleteObject(
            [FromQuery] string dn,  // DistinguishedName
            [FromQuery] string? domain = null)
        {
            ISuccessResult result = this.DeleteService.Delete(dn, domain, this.HttpContext.User);

            return result.Success
                ? this.Ok(result)
                : this.BadRequest(result);
        }
    }
}
