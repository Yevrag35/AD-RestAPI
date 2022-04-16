using AD.Api.Ldap.Search;
using AD.Api.Settings;
using Microsoft.Win32.SafeHandles;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface IQueryService
    {
        List<FindResult> Search(QueryOptions options, out string ldapFilter, out string host);
    }

    public class LdapQueryService : IQueryService
    {
        private IConnectionService Connections { get; }
        private IIdentityService Identity { get; }
        private ISerializationService Serializer { get; }

        public LdapQueryService(IConnectionService connectionService, ISerializationService serializationService, 
            IIdentityService identityService)
        {
            this.Connections = connectionService;
            this.Identity = identityService;
            this.Serializer = serializationService;
        }

        public List<FindResult> Search(QueryOptions options, out string ldapFilter, out string host)
        {
            host = string.Empty;
            ldapFilter = string.Empty;
            using (var connection = this.Connections.GetConnection(options.Domain, options.SearchBase))
            {
                host = connection.SearchBase.Host;
                using (var searcher = connection.CreateSearcher())
                {
                    var list = ExecuteSearch(searcher, options, out ldapFilter);
                    this.Serializer.PrepareMany(list);

                    return list;
                }
            }
        }

        private List<FindResult> ExecuteSearch(Searcher searcher, QueryOptions options, out string ldapFilter)
        {
            ldapFilter = string.Empty;
            (List<FindResult> results, string ldapFilter) function()
            {
                if (options is null)
                    return (new List<FindResult>(), string.Empty);

                List<FindResult> results = searcher.FindAll(options, out string ldapFilter);
                return (results, ldapFilter);
            }

            List<FindResult> results;
            if (!this.Identity.TryGetKerberosIdentity(options?.Principal, out WindowsIdentity? wid))
            {
                (results, ldapFilter) = function();
            }
            else
            {
                (results, ldapFilter) = WindowsIdentity.RunImpersonated(wid.AccessToken, () => function());
            }

            return results;
        }
    }
}
