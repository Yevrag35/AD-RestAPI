using AD.Api.Ldap;
using AD.Api.Ldap.Components;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Search;
using AD.Api.Services;
using AD.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.Resource;
using System.DirectoryServices;
using System.Web;

namespace AD.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("search/user")]
    //[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    public class UserQueryController : ADQueryController
    {
        private static readonly Equal UserObjectClass = new("objectClass", "user");
        private static readonly Equal UserObjectCategory = new("objectCategory", "person");
        private static readonly Equal[] _criteria = new Equal[2] { UserObjectClass, UserObjectCategory };

        private IUserSettings UserSettings { get; }

        public UserQueryController(IConnectionService connectionService,
            IUserSettings userSettings, ISerializationService serializer)
            : base(connectionService, serializer)
        {
            this.UserSettings = userSettings;
        }

        [HttpGet]
        public IActionResult GetUserSearch(
                [FromQuery] string? domain = null,
                [FromQuery] string? searchBase = null,
                [FromQuery] int? limit = null,
                [FromQuery] SearchScope scope = SearchScope.Subtree,
                [FromQuery] string? sortDir = null,
                [FromQuery] string? sortBy = null,
                [FromQuery] string? properties = null)
        {
            return this.PerformUserSearch(
                AddUserCriteria(),
                domain,
                searchBase,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        [HttpPost]
        public IActionResult PostUserSearch(
            [FromBody] IFilterStatement filter,
            [FromQuery] string? domain = null,
            [FromQuery] string? searchBase = null,
            [FromQuery] int? limit = null,
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] string? sortDir = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null)
        {
            return this.PerformUserSearch(
                AddUserCriteria(filter),
                domain,
                searchBase,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        private IActionResult PerformUserSearch(
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
                PropertiesToLoad = GetProperties(this.UserSettings, properties),
                SizeLimit = limit ?? this.UserSettings.Size
            };

            using (var connection = this.GetConnection(domain, searchBase))
            {
                var list = base.PerformSearch(connection, options, out string ldapFilter);

                return base.GetReply(list, options.SizeLimit, options.PropertiesToLoad, connection, ldapFilter);
            }
        }

        private static IFilterStatement AddUserCriteria(IFilterStatement? statement = null)
        {
            return AddCriteria(_criteria, statement);
        }
    }
}
