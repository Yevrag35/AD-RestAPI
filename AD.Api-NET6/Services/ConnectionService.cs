using AD.Api.Domains;
using AD.Api.Ldap;
using AD.Api.Ldap.Connection;

namespace AD.Api.Services
{
    public interface IConnectionService
    {
        LdapConnection GetDefaultConnection();
        LdapConnection GetConnection(string? domain = null);
    }

    public class ConnectionService : IConnectionService
    {
        private SearchDomains Domains { get; }

        public ConnectionService(SearchDomains searchDomains)
        {
            this.Domains = searchDomains;
        }

        public LdapConnection GetDefaultConnection()
        {
            SearchDomain? defDom = this.Domains.GetDefaultDomain();
            if (defDom is null)
                throw new ArgumentNullException("Default Domain");

            return GetConnection(defDom);
        }

        public LdapConnection GetConnection(string? domain = null)
        {
            if (string.IsNullOrWhiteSpace(domain) || !this.Domains.TryGetValue(domain, out SearchDomain? searchDomain))
                return this.GetConnection();

            return GetConnection(searchDomain);
        }

        private static LdapConnection GetConnection(SearchDomain defDom)
        {
            string? host = !string.IsNullOrWhiteSpace(defDom.StaticDomainController)
                ? defDom.StaticDomainController
                : defDom.FQDN;

            return new LdapConnectionBuilder()
                .UsingHost(host)
                .UsingSearchBase(defDom.DistinguishedName)
                .UsingGlobalCatalog()
                .UsingSSL()
                .Build();
        }
    }
}
