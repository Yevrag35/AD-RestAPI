using AD.Api.Ldap.Components;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Search;
using AD.Api.Services;
using AD.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;

namespace AD.Api.Controllers.Search
{
    [ApiController]
    [Produces("application/json")]
    public class GenericQueryController : ADQueryController
    {
        private IGenericSettings GenericSettings { get; }
        
        public GenericQueryController(IIdentityService identityService, IQueryService queryService,
            IResultService resultService, IGenericSettings genericSettings)
            : base(identityService, queryService, resultService)
        {
            this.GenericSettings = genericSettings;
        }

        [HttpGet]
        [Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QueryResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetGenericSearch(
                [FromQuery] string? searchBase = null,
                [FromQuery] string? sortDir = null,
                [FromQuery] string? domain = null,
                [FromQuery] int? limit = null,
                [FromQuery] SearchScope scope = SearchScope.Subtree,
                [FromQuery] string? sortBy = null,
                [FromQuery] string? properties = null,
                [FromQuery] bool includeDetails = false)
        {
            return this.PerformGenericSearch(
                null,
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
        [Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QueryResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PostGenericSearch(
            [FromBody] IFilterStatement filter,
            [FromQuery] string? searchBase = null,
            [FromQuery] string? sortDir = null,
            [FromQuery] string? domain = null,
            [FromQuery] int? limit = null,
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null,
            [FromQuery] bool includeDetails = false)
        {
            return this.PerformGenericSearch(
                filter,
                domain,
                searchBase,
                limit,
                scope,
                sortDir,
                sortBy,
                properties,
                includeDetails);
        }

        private IActionResult PerformGenericSearch(
            IFilterStatement? filter,
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
                PropertiesToLoad = this.GetProperties(this.GenericSettings, properties),
                SizeLimit = limit ?? this.GenericSettings.Size
            };

            var list = this.QueryService.Search(options, out string ldapFilter, out string host);
            return base.GetReply(list, includeDetails, options.SizeLimit, options.PropertiesToLoad, host, ldapFilter);
        }
    }
}
