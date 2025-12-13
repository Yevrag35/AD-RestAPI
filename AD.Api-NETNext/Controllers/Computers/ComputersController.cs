using AD.Api.Attributes;
using AD.Api.Authentication;
using AD.Api.Binding.Attributes;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Computers;
using AD.Api.Core.Security;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Computers;

[ApiAuthorize]
[ApiController]
[Route(ROUTE_NAME)]
public sealed class ComputersController : ControllerBase
{
	private const string ROUTE_NAME = "computers";

	public IComputerSearcher ComputerSearcher { get; }

	public ComputersController(IComputerSearcher computerSearcher)
	{
		this.ComputerSearcher = computerSearcher;
	}

	[HttpGet]
	[Route("{sid:objectsid}")]
	[JwtAuth(AuthorizedRole.Reader)]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CollectionResponse))]
	[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateErrorBody))]
	public IActionResult GetComputer(
		[FromQuery] SearchParameters parameters,
		[FromRouteSid] SidString sid)
	{
		return this.ComputerSearcher.FindOne(sid, parameters, this.HttpContext.RequestServices);
	}
}