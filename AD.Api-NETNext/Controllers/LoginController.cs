using AD.Api.Authentication;
using AD.Api.Core.Authentication.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers
{
    [Route("login")]
    [ApiController]
    [AllowAnonymous]
    public sealed class LoginController : ControllerBase
    {
        private readonly IJwtService _jwtSvc;

        public LoginController(IJwtService jwtSvc)
        {
            _jwtSvc = jwtSvc;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginBody body)
        {
            var oneOf = _jwtSvc.CreateToken(body);
            return oneOf.Match(
                success => new OkObjectResult(new { Key = success }),
                fail => fail);
        }
    }
}
