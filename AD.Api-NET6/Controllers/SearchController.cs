using Microsoft.AspNetCore.Mvc;
using AD.Api.Services;
using AD.Api.Ldap.Filters;
using AD.Api.Settings;
using Microsoft.Extensions.Options;
using AD.Api.Ldap.Search;

namespace AD.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class SearchController : ControllerBase
    {
        private SearchSettings SearchSettings { get; }
        private IConnectionService Connections { get; }

        public SearchController(IConnectionService connectionService, IOptions<SearchSettings> settings)
        {
            this.SearchSettings = settings.Value;
            this.Connections = connectionService;
        }

        [HttpPost]
        [Route("{controller}")]
        public IActionResult PerformSearch([FromBody] IFilterStatement filter)
        {
            using (var connection = this.Connections.GetDefaultConnection())
            {
                SearchOptions opts = new SearchOptions
                {
                    Filter = filter,
                    PropertiesToLoad = this.SearchSettings.DefaultProperties
                };

                using (var searcher = connection.CreateSearcher(opts))
                {
                    var list = searcher.FindAll();

                    return list.Count > 0
                        ? Ok(new
                        {
                            Host = connection.RootDSE.Host ?? "AutoDCLookup",
                            list.Count,
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
                SearchOptions opts = new SearchOptions
                {
                    Filter = filter,
                    PropertiesToLoad = this.SearchSettings.DefaultProperties
                };

                using (var searcher = connection.CreateSearcher(opts))
                {
                    var list = searcher.FindAll();
                    return list.Count > 0
                        ? Ok(new
                        {
                            Host = connection.RootDSE.Host ?? "AutoDCLookup",
                            list.Count,
                            Results = list
                        })
                        : NotFound();
                }
            }
        }
    }
}
