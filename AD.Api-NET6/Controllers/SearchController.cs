using Microsoft.AspNetCore.Mvc;
using AD.Api.Services;
using AD.Api.Ldap.Filters;

namespace AD.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class SearchController : ControllerBase
    {
        private IConnectionService Connections { get; }

        public SearchController(IConnectionService connectionService)
        {
            this.Connections = connectionService;
        }

        [HttpPost]
        [Route("{controller}")]
        public IActionResult PerformSearch([FromBody] IFilterStatement filter)
        {
            using (var connection = this.Connections.GetDefaultConnection())
            {
                using (var searcher = connection.CreateSearcher(filter))
                {
                    var list = searcher.FindAll();

                    return list.Count > 0
                        ? Ok(new
                        {
                            Host = connection.RootDSE.Host ?? "AutoDCLookup",
                            Count = list.Count,
                            Results = list
                        })
                        : NotFound();
                }
            }
        }

        [HttpPost]
        [Route("{controller}/{domain}")]
        public IActionResult PerformSearch(string domain, [FromBody] IFilterStatement filter)
        {
            using (var connection = this.Connections.GetConnection(domain))
            {
                using (var searcher = connection.CreateSearcher(filter))
                {
                    var list = searcher.FindAll();
                    return list.Count > 0
                        ? Ok(list)
                        : NotFound();
                }
            }
        }
    }
}
