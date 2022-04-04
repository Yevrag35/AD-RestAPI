using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Search;
using AD.Api.Services;
using AD.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.DirectoryServices;
using System.Web;

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
        public IActionResult PerformSearch([FromBody] IFilterStatement filter, [FromQuery] int limit = 0, 
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] SortDirection sortDir = SortDirection.Ascending,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null)
        {
            string[]? propertiesToAdd = SplitProperties(properties) ?? this.SearchSettings.Properties;

            using (var connection = this.Connections.GetDefaultConnection())
            {
                SearchOptions opts = new SearchOptions
                {
                    Filter = filter,
                    SizeLimit = limit,
                    SearchScope = scope,
                    SortDirection = sortDir,
                    SortProperty = sortBy,
                    PropertiesToLoad = propertiesToAdd
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
                    PropertiesToLoad = this.SearchSettings.Properties
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

        private static string[]? SplitProperties(string? propertiesStr)
        {
            return propertiesStr?.Split(new char[2] { (char)32, (char)43 }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
