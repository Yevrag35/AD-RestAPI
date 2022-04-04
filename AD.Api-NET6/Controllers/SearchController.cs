using AD.Api.Ldap;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Models;
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
        private IComputerSettings ComputerSettings { get; }
        private IGenericSettings GenericSettings { get; }
        private IUserSettings UserSettings { get; }
        private IConnectionService Connections { get; }

        public SearchController(IConnectionService connectionService, IGenericSettings genericSettings,
            IUserSettings userSettings, IComputerSettings computerSettings)
        {
            this.GenericSettings = genericSettings;
            this.UserSettings = userSettings;
            this.ComputerSettings = computerSettings;
            this.Connections = connectionService;
        }

        [HttpPost]
        [Route("{controller}")]
        public IActionResult PerformSearch([FromBody] IFilterStatement filter,
            [FromQuery] string? domain = null,
            [FromQuery] int limit = 0, 
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] SortDirection sortDir = SortDirection.Ascending,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors, (y, z) => z.Exception?.Message);

                return BadRequest(errors);
            }

            string[]? propertiesToAdd = SplitProperties(properties) ?? this.GenericSettings.Properties;


            using (var connection = this.GetConnection(domain))
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

                    return this.GetReply(list, connection);
                }
            }
        }

        [HttpPost]
        [Route("{controller}/users")]
        public IActionResult PerformUserSearch([FromBody] IFilterStatement filter,
            [FromQuery] string? domain = null,
            [FromQuery] int limit = 0,
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] SortDirection sortDir = SortDirection.Ascending,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null)
        {
            filter = new And
            {
                new Equal("objectClass", "user"),
                new Equal("objectCategory", "person"),
                filter
            };

            using (var connection = this.GetConnection(domain))
            {
                SearchOptions opts = new SearchOptions
                {
                    Filter = filter,
                    PropertiesToLoad = this.UserSettings.Properties
                };

                using (var searcher = connection.CreateSearcher(opts))
                {
                    var list = searcher.FindAll();
                    return this.GetReply(list, connection);
                }
            }
        }

        private LdapConnection GetConnection(string? domain)
        {
            return string.IsNullOrWhiteSpace(domain)
                ? this.Connections.GetDefaultConnection()
                : this.Connections.GetConnection(domain);
        }

        private IActionResult GetReply(List<FindResult> list, LdapConnection connection)
        {
            return Ok(new
            {
                Host = connection.RootDSE.Host ?? "AutoDCLookup",
                list.Count,
                Results = list
            });
        }

        private static string[]? SplitProperties(string? propertiesStr)
        {
            return propertiesStr?.Split(new char[2] { (char)32, (char)43 }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
