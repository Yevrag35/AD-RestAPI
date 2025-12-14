using AD.Api.Authentication;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

namespace AD.Api.Controllers;

[Route("login")]
[ApiController]
[Authorize]
public sealed class LoginController : ControllerBase
{
	private readonly IJwtService _jwtSvc;

	public LoginController(IJwtService jwtSvc)
	{
		_jwtSvc = jwtSvc;
	}

	[HttpPost]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BearerToken))]
	[ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(ApiExceptionResult))]
	public async Task<IActionResult> Login([FromBody] LoginBody body)
	{
		var oneOf = _jwtSvc.CreateToken(body);
		return await oneOf.Match(
			success => Task.FromResult<IActionResult>(new OkObjectResult(success)),
			async fail =>
			{
				int randomDelay = RandomNumberGenerator.GetInt32(50, 801);
				await Task.Delay(randomDelay).ConfigureAwait(false);
				return fail;
			})
			.ConfigureAwait(false);
	}
}
