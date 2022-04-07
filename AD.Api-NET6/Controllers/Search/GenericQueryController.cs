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
    [Route("search")]
    public class GenericQueryController : ADQueryController
    {
        private IGenericSettings GenericSettings { get; }
        
        public GenericQueryController(IConnectionService connectionService, IGenericSettings genericSettings,
            ISerializationService serializationService)
            : base(connectionService, serializationService)
        {
            this.GenericSettings = genericSettings;
        }

        [HttpGet]
        public IActionResult GetGenericSearch(
                [FromQuery] string? sortDir = null,
                [FromQuery] string? domain = null,
                [FromQuery] int? limit = null,
                [FromQuery] SearchScope scope = SearchScope.Subtree,
                [FromQuery] string? sortBy = null,
                [FromQuery] string? properties = null)
        {
            return this.PerformGenericSearch(
                null,
                domain,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        [HttpPost]
        public IActionResult PostGenericSearch(
            [FromBody] IFilterStatement filter,
            [FromQuery] string? sortDir = null,
            [FromQuery] string? domain = null,
            [FromQuery] int? limit = null,
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null)
        {
            return this.PerformGenericSearch(
                filter,
                domain,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        private IActionResult PerformGenericSearch(
            IFilterStatement? filter,
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
                PropertiesToLoad = GetProperties(this.GenericSettings, properties),
                SizeLimit = limit ?? this.GenericSettings.Size
            };

            using (var connection = this.GetConnection(domain))
            {
                var list = base.PerformSearch(connection, options, out string ldapFilter);

                return base.GetReply(list, options.SizeLimit, options.PropertiesToLoad, connection, ldapFilter);
            }
        }
    }
}
