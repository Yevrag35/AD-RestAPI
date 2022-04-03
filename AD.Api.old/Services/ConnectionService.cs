using AD.Api.Components;
using AD.Api.Ldap;
using AD.Api.Ldap.Connection;

namespace AD.Api.Services
{
    public interface IConnectionService
    {
        LdapConnection GetConnection();
        LdapConnection GetConnection(string? domain);
    }

    public class ConnectionService : IConnectionService
    {
        private SearchDomains Domains { get; }

        public ConnectionService(SearchDomains searchDomains)
        {
            this.Domains = searchDomains;
        }

        public LdapConnection GetConnection()
        {
            SearchDomain defDom = this.Domains.GetDefaultDomain();
            return this.GetConnection(defDom);
        }

        public LdapConnection GetConnection(string? domain)
        {
            if (string.IsNullOrWhiteSpace(domain) || !this.Domains.TryGetValue(domain, out SearchDomain searchDomain))
                return this.GetConnection();

            return this.GetConnection(searchDomain);
        }

        private LdapConnection GetConnection(SearchDomain defDom)
        {
            string host = !string.IsNullOrWhiteSpace(defDom.StaticDomainController)
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
