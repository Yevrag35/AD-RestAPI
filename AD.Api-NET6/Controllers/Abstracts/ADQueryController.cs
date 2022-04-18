using AD.Api.Ldap;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Search;
using AD.Api.Services;
using AD.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.Security.Claims;

namespace AD.Api.Controllers
{
    public abstract class ADQueryController : ADControllerBase
    {
        protected IQueryService QueryService { get; }
        private IResultService ResultService { get; }

        public ADQueryController(IIdentityService identityService, IQueryService queryService, IResultService resultService)
            : base(identityService)
        {
            this.QueryService = queryService;
            this.ResultService = resultService;
        }

        protected static IFilterStatement AddCriteria<T>(IList<T> criteria, IFilterStatement? existingFilter)
            where T : IFilterStatement
        {
            if (existingFilter is not And and)
            {
                and = new And
                {
                    existingFilter
                };
            }

            for (int i = 0; i < criteria.Count; i++)
            {
                IFilterStatement criterion = criteria[i];
                if (!and.Contains(criterion))
                    and.Add(criterion);
            }

            return and;
        }

        protected IActionResult GetReply(List<FindResult>? list, string host)
        {
            return Ok(this.ResultService.GetQueryReply(list, host));
        }
        protected IActionResult GetReply(List<FindResult>? list, bool includeRequestDetails, int limitedTo, IList<string>? propertiesRequested,
            string host, string? ldapFilter = null)
        {
            return !includeRequestDetails
                ? this.GetReply(list, host)
                : Ok(this.ResultService.GetQueryReply(list, limitedTo, propertiesRequested, host, ldapFilter));
        }

        protected string[] GetProperties(ISearchSettings options, string? askedForProperties)
        {
            string[]? split = SplitProperties(askedForProperties);
            if (split is not null && split.Contains("default", StringComparer.CurrentCultureIgnoreCase))
            {
                return AddProperties(split, options.Properties);
            }
            else
            {
                return split ?? options.Properties ?? Array.Empty<string>();
            }
        }

        private static string[] AddProperties(string[] askedFor, string[]? defaultProperties)
        {
            if (defaultProperties is null)
                defaultProperties = Array.Empty<string>();

            string[] newArr = new string[askedFor.Length - 1 + defaultProperties.Length];
            int index = 0;
            for (int i = 0; i < askedFor.Length; i++)
            {
                string s = askedFor[i];
                if (s.Equals("default", StringComparison.CurrentCultureIgnoreCase))
                    continue;                    

                newArr[i - index] = s;
                index++;
            }

            for (int n = 0; n < defaultProperties.Length; n++)
            {
                newArr[n + index] = defaultProperties[n];
            }

            return newArr;
        }

        private static string[]? SplitProperties(string? propertiesStr)
        {
            return propertiesStr?.Split(new char[2] { (char)32, (char)43 }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
