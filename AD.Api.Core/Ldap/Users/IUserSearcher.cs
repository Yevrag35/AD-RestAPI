using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Security;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Users
{
    public interface IUserSearcher
    {
        [SupportedOSPlatform("WINDOWS")]
        IActionResult GetOneUser(SidString userSid, SearchParameters parameters, IServiceProvider provider);
    }

    [DependencyRegistration(typeof(IUserSearcher), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class UserSearcher : IUserSearcher
    {
        private readonly IRequestService _requestSvc;
        private readonly ILdapFilterService _filterSvc;
        
        public UserSearcher(ILdapFilterService filterSvc, IRequestService requestSvc)
        {
            _filterSvc = filterSvc;
            _requestSvc = requestSvc;
        }

        [SupportedOSPlatform("WINDOWS")]
        public IActionResult GetOneUser(SidString userSid, SearchParameters parameters, IServiceProvider provider)
        {
            string filter = _filterSvc.GetFilter(userSid, FilteredRequestType.User);
            SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.User);

            parameters.ApplyParameters(searchFilter);
            return _requestSvc.FindOne(parameters, provider);
        }
    }
}
