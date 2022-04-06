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
    [Route("search/user")]
    public class UserQueryController : ADQueryController
    {
        private static readonly Equal UserObjectClass = new Equal("objectClass", "user");
        private static readonly Equal UserObjectCategory = new Equal("objectCategory", "person");

        private IGenericSettings GenericSettings { get; }
        private IUserSettings UserSettings { get; }

        public UserQueryController(IConnectionService connectionService, IGenericSettings genericSettings,
            IUserSettings userSettings, ISerializationService serializer)
            : base(connectionService, serializer)
        {
            this.GenericSettings = genericSettings;
            this.UserSettings = userSettings;
        }

        [HttpGet]
        public IActionResult GetUserSearch(
                [FromQuery] string? domain = null,
                [FromQuery] int? limit = null,
                [FromQuery] SearchScope scope = SearchScope.Subtree,
                [FromQuery] SortDirection sortDir = SortDirection.Ascending,
                [FromQuery] string? sortBy = null,
                [FromQuery] string? properties = null)
        {
            return this.PerformUserSearch(
                AddUserCriteria(),
                domain,
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
            [FromQuery] int? limit = null,
            [FromQuery] SearchScope scope = SearchScope.Subtree,
            [FromQuery] SortDirection sortDir = SortDirection.Ascending,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? properties = null)
        {
            return this.PerformUserSearch(
                AddUserCriteria(filter),
                domain,
                limit,
                scope,
                sortDir,
                sortBy,
                properties);
        }

        private IActionResult PerformUserSearch(
            IFilterStatement filter,
            string? domain,
            int? limit,
            SearchScope scope,
            SortDirection sortDir,
            string? sortBy,
            string? properties)
        {
            SearchOptions opts = new SearchOptions
            {
                Filter = filter,
                SearchScope = scope,
                SortDirection = sortDir,
                SortProperty = sortBy,
                PropertiesToLoad = GetProperties(this.UserSettings, properties),
                SizeLimit = limit ?? this.UserSettings.Size
            };

            using (var connection = this.GetConnection(domain))
            {
                var list = base.PerformSearch(connection, opts);

                return base.GetReply(list, connection);
            }
        }

        private static IFilterStatement AddUserCriteria(IFilterStatement? statement = null)
        {
            return new And
            {
                UserObjectClass,
                UserObjectCategory,
                statement
            };
        }
    }
}
