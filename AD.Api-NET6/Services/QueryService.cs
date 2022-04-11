using AD.Api.Ldap.Search;
using AD.Api.Settings;

namespace AD.Api.Services
{
    public interface IQueryService
    {
        List<FindResult> Search(QueryOptions options, out string ldapFilter, out string host);
    }

    public class LdapQueryService : IQueryService
    {
        private IConnectionService Connections { get; }
        private ISerializationService Serializer { get; }

        public LdapQueryService(IConnectionService connectionService, ISerializationService serializationService)
        {
            this.Connections = connectionService;
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
                    var list = searcher.FindAll(options, out ldapFilter);
                    this.Serializer.PrepareMany(list);

                    return list;
                }
            }
        }
    }
}
