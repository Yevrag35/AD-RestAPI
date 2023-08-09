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

            using var connection = this.Connections.GetConnection(con =>
            {
                con.Domain = options.Domain;
                con.DontDisposeHandle = false;
                con.SearchBase = options.SearchBase;
                con.Principal = options.ClaimsPrincipal;
            });

            using var searcher = connection.CreateSearcher();
            var list = this.ExecuteSearch(searcher, options, out ldapFilter, out string? hostContacted);
            host = hostContacted ?? connection.SearchBase.Host;

            this.Serializer.PrepareMany(list);

            return list;
        }

        private List<FindResult> ExecuteSearch(ILdapSearcher searcher, QueryOptions options, out string ldapFilter, out string? hostContacted)
        {
            ldapFilter = string.Empty;
            (List<FindResult> results, string ldapFilter, string? hostContacted) function()
            {
                if (options is null)
                {
                    return (new List<FindResult>(), string.Empty, null);
                }

                List<FindResult> results = searcher.FindAll(options, out string ldapFilter, out string? hostContacted);
                return (results, ldapFilter, hostContacted);
            }

            List<FindResult> results;
            if (!this.Identity.TryGetKerberosIdentity(options?.ClaimsPrincipal, out WindowsIdentity? wid))
            {
                (results, ldapFilter, hostContacted) = function();
            }
            else
            {
                (results, ldapFilter, hostContacted) = WindowsIdentity.RunImpersonated(wid.AccessToken, () => function());
            }

            return results;
        }
    }
}
