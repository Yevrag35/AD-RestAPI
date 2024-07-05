using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Security;
using AD.Api.Pooling;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;
using System.Text.Json;

namespace AD.Api.Core.Ldap.Users
{
    public interface IUserSearcher
    {
        IActionResult GetOneUser(SidString userSid, SearchParameters parameters, IServiceProvider provider);
        OneOf<ConnectedResponse, IActionResult> GetOneUserAndContinue(SidString userSid, in DomainQuery target);
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

        public OneOf<ConnectedResponse, IActionResult> GetOneUserAndContinue(SidString userSid, in DomainQuery target)
        {
            string filter = _filterSvc.GetFilter(userSid, FilteredRequestType.User);
            SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.User);
            SearchParameters parameters = new()
            {
                SearchRequest = target.GetRequiredService<IPooledItem<LdapSearchRequest>>(),
                Info = target,
                SizeLimit = 1,
                Scope = SearchScope.Subtree,
                Properties = AttributeConstants.DISTINGUISHED_NAME,
            };

            parameters.ApplyParameters(searchFilter);

            return _requestSvc.FindOneAndContinue(parameters);
        }
        public IActionResult GetOneUser(SidString userSid, SearchParameters parameters, IServiceProvider provider)
        {
            string filter = _filterSvc.GetFilter(userSid, FilteredRequestType.User);
            SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.User);

            parameters.ApplyParameters(searchFilter);
            
            return _requestSvc.FindOne(parameters, provider);
        }
    }
}
