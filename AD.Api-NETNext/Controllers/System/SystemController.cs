using AD.Api.Core.Ldap;
using AD.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Collections.Frozen;

namespace AD.Api.Controllers.System
{
    [Authorize]
    [ApiController]
    [Route("system")]
    public sealed class SystemController : ControllerBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly WellKnownObjectDictionary _dictionary;

        public SystemController(WellKnownObjectDictionary dictionary)
        {
            _dictionary = dictionary;
        }

        [HttpGet]
        [Route("wellKnownPaths")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<WellKnownObjectValue, string>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WellKnownPathResult))]
        public IActionResult GetWellKnownPaths(
            [FromQuery] string? domain = null,
            [FromQuery] WellKnownObjectValue? key = null)
        {
            if (key.HasValue)
            {
                _logger.Info("Requesting well-known path for {WellKnown}...", key.Value);
                if (!_dictionary.TryGetValue(domain, key.Value, out string? location))
                {
                    location = string.Empty;
                }

                return this.Ok(new WellKnownPathResult
                {
                    DistinguishedName = location,
                    WellKnown = key.Value,
                });
            }

            _logger.Info("Requesting well-known paths...");
            return this.Ok(_dictionary[domain]);
        }
    }
}
