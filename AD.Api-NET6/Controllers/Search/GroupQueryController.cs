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
    [Route("search/group")]
    public class GroupQueryController : ADQueryController
    {
        private static readonly Equal GroupObjectClass = new("objectClass", "group");
        private static readonly Equal[] _criteria = new[] { GroupObjectClass };

        private IGroupSettings GroupSettings { get; }

        public GroupQueryController(IConnectionService connectionService, ISerializationService serializationService,
            IGroupSettings groupSettings)
            : base(connectionService, serializationService)
        {
            this.GroupSettings = groupSettings;
        }

        [HttpGet]
        public IActionResult GetGroupSearch(
                [FromQuery] string? domain = null,
                [FromQuery] int? limit = null,
                [FromQuery] SearchScope scope = SearchScope.Subtree,
                [FromQuery] string? sortDir = null,
                [FromQuery] string? sortBy = null,
                [FromQuery] string? properties = null)
        {
            return this.PerformGroupSearch(
                AddCriteria(_criteria, null),
                domain,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        [HttpPost]
        public IActionResult PostGroupSearch(
            [FromBody] IFilterStatement filter,
            [FromQuery] string? domain = null,
            [FromQuery] int? limit = null,
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] string? sortDir = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null)
        {
            return this.PerformGroupSearch(
                AddCriteria(_criteria, filter),
                domain,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        private IActionResult PerformGroupSearch(
            IFilterStatement filter,
            string? domain,
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
                PropertiesToLoad = GetProperties(
                    this.GroupSettings,
                    properties),
                SizeLimit = limit ?? this.GroupSettings.Size
            };

            if (this.GroupSettings.IncludeMembers && !options.PropertiesToLoad.Contains("member", StringComparer.CurrentCultureIgnoreCase))
                options.PropertiesToLoad.Add("member");

            using (var connection = this.GetConnection(domain))
            {
                var list = base.PerformSearch(connection, options, out string ldapFilter);

                return base.GetReply(list, options.SizeLimit, options.PropertiesToLoad, connection, ldapFilter);
            }
        }
    }
}
