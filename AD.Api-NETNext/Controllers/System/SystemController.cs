using AD.Api.Core.Ldap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace AD.Api.Controllers.System
{
    [Route("system")]
    [ApiController]
    public sealed class SystemController : ControllerBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public SystemController()
        {
        }

        [HttpGet]
        [Route("wellKnownPaths")]
        public IActionResult GetWellKnownPaths(
            [FromServices] WellKnownObjectDictionary dictionary,
            [FromQuery] string? domain = null,
            [FromQuery] WellKnownObjectValue? key = null)
        {
            _logger.Info("Requesting well-known paths...");
            if (key.HasValue)
            {
                _logger.Info("Requesting well-known path for {WellKnown}...", key.Value);
                if (!dictionary.TryGetValue(domain, key.Value, out string? location))
                {
                    location = string.Empty;
                }

                return this.Ok(new
                {
                    WellKnown = key.Value,
                    DistinguishedName = location,
                });
            }

            return this.Ok(dictionary[domain]);
        }
    }
}
