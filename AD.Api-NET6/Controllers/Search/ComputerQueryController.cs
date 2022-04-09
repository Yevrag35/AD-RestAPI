using AD.Api.Ldap.Components;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Search;
using AD.Api.Services;
using AD.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;

namespace AD.Api.Controllers.Search
{
    [ApiController]
    [Produces("application/json")]
    [Route("search/computer")]
    public class ComputerQueryController : ADQueryController
    {
        private static readonly Equal ComputerObjectClass = new Equal("objectClass", "computer");
        private static readonly Equal[] _criteria = new[] { ComputerObjectClass };

        private IComputerSettings ComputerSettings { get; }

        public ComputerQueryController(IConnectionService connectionService,
            IComputerSettings computerSettings, ISerializationService serializer)
            : base(connectionService, serializer)
        {
            this.ComputerSettings = computerSettings;
        }

        [HttpGet]
        public IActionResult GetComputerSearch(
                [FromQuery] string? domain = null,
                [FromQuery] string? searchBase = null,
                [FromQuery] int? limit = null,
                [FromQuery] SearchScope scope = SearchScope.Subtree,
                [FromQuery] string? sortDir = null,
                [FromQuery] string? sortBy = null,
                [FromQuery] string? properties = null)
        {
            return this.PerformComputerSearch(
                AddCriteria(_criteria, null),
                domain,
                searchBase,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        [HttpPost]
        public IActionResult PostComputerSearch(
            [FromBody] IFilterStatement filter,
            [FromQuery] string? domain = null,
            [FromQuery] string? searchBase = null,
            [FromQuery] int? limit = null,
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] string? sortDir = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null)
        {
            return this.PerformComputerSearch(
                AddCriteria(_criteria, filter),
                domain,
                searchBase,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        private IActionResult PerformComputerSearch(
            IFilterStatement filter,
            string? domain,
            string? searchBase,
            int? limit,
            SearchScope scope,
            PropertySortDirection sortDir,
            string? sortBy,
            string? properties)
        {
            SearchOptions options = new SearchOptions
            {
                Filter = filter,
                SearchScope = scope,
                SortDirection = sortDir,
                SortProperty = sortBy,
                PropertiesToLoad = GetProperties(this.ComputerSettings, properties),
                SizeLimit = limit ?? this.ComputerSettings.Size
            };

            using (var connection = this.GetConnection(domain, searchBase))
            {
                var list = base.PerformSearch(connection, options, out string ldapFilter);

                return base.GetReply(list, options.SizeLimit, options.PropertiesToLoad, connection, ldapFilter);
            }
        }
    }
}
