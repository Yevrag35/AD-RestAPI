using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;

namespace AD.Api.Controllers.Search
{
    [ApiController]
    [Produces("application/json")]
    [Route("search/group")]
    public class GroupQueryController : ADQueryController
    {
        private static readonly Equal GroupObjectClass = new("objectClass", "group");
        private static readonly Equal[] _criteria = new[] { GroupObjectClass };

        private IGroupSettings GroupSettings { get; }

        public GroupQueryController(IIdentityService identityService, IQueryService queryService,
            IResultService resultService, IGroupSettings groupSettings)
            : base(identityService, queryService, resultService)
        {
            this.GroupSettings = groupSettings;
        }

        [HttpGet]
        public IActionResult GetGroupSearch(
                [FromQuery] string? domain = null,
                [FromQuery] string? searchBase = null,
                [FromQuery] int? limit = null,
                [FromQuery] SearchScope scope = SearchScope.Subtree,
                [FromQuery] string? sortDir = null,
                [FromQuery] string? sortBy = null,
                [FromQuery] string? properties = null,
                [FromQuery] bool includeDetails = false)
        {
            return this.PerformGroupSearch(
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
        public IActionResult PostGroupSearch(
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
            return this.PerformGroupSearch(
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

        private IActionResult PerformGroupSearch(
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
                Filter = filter,
                SearchScope = scope,
                SortDirection = sortDir,
                SortProperty = sortBy,
                ClaimsPrincipal = this.HttpContext.User,
                PropertiesToLoad = this.GetProperties(
                    this.GroupSettings,
                    properties),
                SizeLimit = limit ?? this.GroupSettings.Size
            };

            if (this.GroupSettings.IncludeMembers && !options.PropertiesToLoad.Contains("member", StringComparer.CurrentCultureIgnoreCase))
            {
                options.PropertiesToLoad.Add("member");
            }

            var list = this.QueryService.Search(options, out string ldapFilter, out string host);
            return base.GetReply(list, includeDetails, options.SizeLimit, options.PropertiesToLoad, host, ldapFilter);
        }
    }
}
