using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Security;
using AD.Api.Pooling;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;

namespace AD.Api.Core.Ldap.Users
{
    public interface IUserSearcher
    {
        IActionResult GetOneUser(SidString userSid, SearchParameters parameters, IServiceProvider provider);
        /// <summary>
        /// Retrieves a single user object by its object SID from the specified target domain and returns the result
        /// along with the active connection for sending further queries.
        /// </summary>
        /// <param name="userSid">The user object's SID to search for.</param>
        /// <param name="target">The target domain and/or domain controller to send the request to.</param>
        /// <param name="extraProperties"></param>
        /// <returns>
        /// A <see cref="ConnectedResponse"/> object containing the distinguishedName of the found user object - or -
        /// an <see cref="IActionResult"/> containing the web response result if the operation failed or was unable to
        /// find the user object.
        /// </returns>
        OneOf<ConnectedResponse, IActionResult> GetOneUserAndContinue(SidString userSid, in DomainQuery target, string[]? extraProperties = null);
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
        
        public IActionResult GetOneUser(SidString userSid, SearchParameters parameters, IServiceProvider provider)
        {
            string filter = _filterSvc.GetFilter(userSid, FilteredRequestType.User);
            SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.User);

            parameters.ApplyParameters(searchFilter);
            
            return _requestSvc.FindOne(parameters, provider);
        }
        public OneOf<ConnectedResponse, IActionResult> GetOneUserAndContinue(SidString userSid, in DomainQuery target, string[]? extraProperties = null)
        {
            string filter = _filterSvc.GetFilter(userSid, FilteredRequestType.User);
            SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.User);
            SearchParameters parameters = new()
            {
                SearchRequest = target.GetRequiredService<IPooledItem<LdapSearchRequest>>(),
                Info = target,
                SizeLimit = 1,
                Scope = SearchScope.Subtree,
            };

            SetProperties(parameters, extraProperties);

            parameters.ApplyParameters(searchFilter);

            return _requestSvc.FindOneAndContinue(parameters);
        }

        private static void SetProperties(SearchParameters parameters, string[]? extraProperties)
        {
            if (extraProperties is null || extraProperties.Length == 0)
            {
                parameters.PropertiesArray = [];
                parameters.Properties = AttributeConstants.DISTINGUISHED_NAME;
                return;
            }

            string[] atts = new string[extraProperties.Length + 1];
            atts[0] = AttributeConstants.DISTINGUISHED_NAME;
            extraProperties.CopyTo(atts, 1);

            parameters.Properties = null;
            parameters.PropertiesArray = atts;
        }
    }
}
