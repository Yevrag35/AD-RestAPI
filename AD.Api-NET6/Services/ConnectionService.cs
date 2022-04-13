using AD.Api.Domains;
using AD.Api.Ldap;
using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Path;
using AD.Api.Schema;

namespace AD.Api.Services
{
    public interface IConnectionService
    {
        LdapConnection GetDefaultConnection(string? searchBase = null);
        LdapConnection GetConnection(string? domain = null);
        LdapConnection GetConnection(string? domain, string? searchBase);
    }

    public class ConnectionService : IConnectionService
    {
        private SearchDomains Domains { get; }

        public ConnectionService(SearchDomains searchDomains)
        {
            this.Domains = searchDomains;
            //if (!SchemaCache.IsLoaded)
            //{
            //    using var connection = this.GetDefaultConnection();
            //    SchemaCache.LoadSchema(connection.GetForestContext(), new string[] { "user", "group", "computer", "contact", "organizationalUnit" });
            //}
        }

        public LdapConnection GetDefaultConnection(string? searchBase = null)
        {
            SearchDomain? defDom = this.Domains.GetDefaultDomain();
            if (defDom is null)
                throw new ArgumentNullException("Default Domain");

            return GetConnection(defDom, searchBase);
        }

        public LdapConnection GetConnection(string? domain = null)
        {
            if (string.IsNullOrWhiteSpace(domain) || !this.Domains.TryGetValue(domain, out SearchDomain? searchDomain))
                return this.GetDefaultConnection();

            return GetConnection(searchDomain, null);
        }

        public LdapConnection GetConnection(string? domain, string? searchBase)
        {
            if (string.IsNullOrWhiteSpace(domain) || !this.Domains.TryGetValue(domain, out SearchDomain? searchDomain))
                return this.GetDefaultConnection(searchBase);

            return GetConnection(searchDomain, searchBase);
        }

        private static LdapConnection GetConnection(SearchDomain defDom, string? searchBase)
        {
            string? host = !string.IsNullOrWhiteSpace(defDom.StaticDomainController)
                ? defDom.StaticDomainController
                : defDom.FQDN;

            if (string.IsNullOrWhiteSpace(searchBase))
                searchBase = defDom.DistinguishedName;

            return new LdapConnectionBuilder()
                .UsingHost(host)
                .UsingSearchBase(searchBase)
                .UsingGlobalCatalog(defDom.UseGlobalCatalog)
                .UsingForestControls(defDom.IsForestRoot)
                .UseSchemaCache(defDom.UseSchemaCache)
                .UsingSSL(defDom.UseSSL)
                .Build();
        }
    }
}
