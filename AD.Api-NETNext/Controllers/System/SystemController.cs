using AD.Api.Authentication;
using AD.Api.Binding.Attributes;
using AD.Api.Core;
using AD.Api.Core.Authentication;
using AD.Api.Core.Ldap;
using AD.Api.Models;
using Microsoft.AspNetCore.Authorization;
using NLog;

namespace AD.Api.Controllers.System;

[Authorize]
[ApiController]
[Route("system")]
public sealed class SystemController : ControllerBase
{
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
	private readonly IWellKnownService _wkSvc;

	public SystemController(IWellKnownService wkSvc)
	{
		_wkSvc = wkSvc;
	}

	[HttpGet]
	[Route("wellKnownPaths")]
	[JwtAuth(AuthorizedRole.Reader)]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<WellKnownObjectValue, string>))]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WellKnownPathResult))]
	public IActionResult GetWellKnownPaths(
		[Domain] DomainQuery domain,
		[FromQuery] WellKnownObjectValue? key = null)
	{
		if (key.HasValue)
		{
			_logger.Info("Requesting well-known path for {WellKnown}...", key.Value);
			DistinguishedName location = _wkSvc.GetValueByKey(domain.Domain, key.Value);

			return this.Ok(new WellKnownPathResult
			{
				DistinguishedName = location,
				WellKnown = key.Value,
			});
		}

		_logger.Info("Requesting all well-known paths...");
		var array = _wkSvc.GetAllWellKnownsInDomain(domain.Domain);
		return this.Ok(array);
	}
}
