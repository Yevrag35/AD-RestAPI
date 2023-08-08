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

        public ComputerQueryController(IIdentityService identityService, IQueryService queryService,
            IResultService resultService, IComputerSettings computerSettings)
            : base(identityService, queryService, resultService)
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
                [FromQuery] string? properties = null,
                [FromQuery] bool includeDetails = false)
        {
            return this.PerformComputerSearch(
                AddCriteria(_criteria, null),
                domain,
                searchBase,
                limit,
                scope,
                sortDir,
                sortBy,
                properties,
                includeDetails);
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
            [FromQuery] string? properties = null,
            [FromQuery] bool includeDetails = false)
        {
            return this.PerformComputerSearch(
                AddCriteria(_criteria, filter),
                domain,
                searchBase,
                limit,
                scope,
                sortDir,
                sortBy,
                properties,
                includeDetails);
        }

        private IActionResult PerformComputerSearch(
            IFilterStatement filter,
            string? domain,
            string? searchBase,
            int? limit,
            SearchScope scope,
            PropertySortDirection sortDir,
            string? sortBy,
            string? properties,
            bool includeDetails)
        {
            QueryOptions options = new QueryOptions
            {
                Domain = domain,
                SearchBase = searchBase,
                Filter = filter,
                SearchScope = scope,
                SortDirection = sortDir,
                SortProperty = sortBy,
                ClaimsPrincipal = this.HttpContext.User,
                PropertiesToLoad = this.GetProperties(this.ComputerSettings, properties),
                SizeLimit = limit ?? this.ComputerSettings.Size
            };

            var list = this.QueryService.Search(options, out string ldapFilter, out string host);
            return base.GetReply(list, includeDetails, options.SizeLimit, options.PropertiesToLoad, host, ldapFilter);
        }
    }
}
