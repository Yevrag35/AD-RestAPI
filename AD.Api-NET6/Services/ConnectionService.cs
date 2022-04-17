using AD.Api.Domains;
using AD.Api.Ldap;
using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Path;
using AD.Api.Schema;
using Microsoft.Win32.SafeHandles;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface IConnectionService
    {
        //[Obsolete($"Use the overload with {nameof(ConnectionOptions)}.")]
        //LdapConnection GetDefaultConnection(string? searchBase = null);
        LdapConnection GetConnection(Action<ConnectionOptions> configureOptions);
        LdapConnection GetConnection(IConnectionOptions options);
        //[Obsolete($"Use the overload with {nameof(ConnectionOptions)}.")]
        //LdapConnection GetConnection(string? domain = null);
        //[Obsolete($"Use the overload with {nameof(ConnectionOptions)}.")]
        //LdapConnection GetConnection(string? domain, string? searchBase);
    }

    public class ConnectionService : IConnectionService
    {
        private SearchDomains Domains { get; }
        private IIdentityService Identity { get; }

        public ConnectionService(IIdentityService identityService, SearchDomains searchDomains)
        {
            this.Identity = identityService;
            this.Domains = searchDomains;
        }

        //[Obsolete($"Use the overload with {nameof(ConnectionOptions)}.")]
        //public LdapConnection GetDefaultConnection(string? searchBase = null)
        //{
        //    SearchDomain? defDom = this.Domains.GetDefaultDomain();
        //    if (defDom is null)
        //        throw new ArgumentNullException("Default Domain");

        //    return GetConnection(defDom, searchBase);
        //}

        public LdapConnection GetConnection(Action<ConnectionOptions> configureOptions)
        {
            var options = new ConnectionOptions();
            configureOptions(options);

            return this.GetConnection(options);
        }

        public LdapConnection GetConnection(IConnectionOptions options)
        {
            SafeAccessTokenHandle? token = null;
            if (this.Identity.TryGetKerberosIdentity(options.Principal, out WindowsIdentity? identity))
            {
                token = identity.AccessToken;
            }

            SearchDomain searchDomain = !string.IsNullOrWhiteSpace(options.Domain) 
                                        && 
                                        this.Domains.TryGetValue(options.Domain, out SearchDomain? thisDomain)
                ? thisDomain
                : this.Domains.GetDefaultDomain()
                    ?? throw new ArgumentException("Domain is not specified and there is no default defined.");

            string host = !string.IsNullOrWhiteSpace(searchDomain.StaticDomainController)
                ? searchDomain.StaticDomainController
                : searchDomain.FQDN;

            string? searchBase = string.IsNullOrWhiteSpace(options.SearchBase)
                ? searchDomain.DistinguishedName
                : options.SearchBase;

            return new LdapConnectionBuilder()
                .UsingHost(host)
                .UsingSearchBase(searchBase)
                .UsingGlobalCatalog(searchDomain.UseGlobalCatalog)
                .UsingForestControls(searchDomain.IsForestRoot)
                .UsingIdentity(token, options.DontDisposeHandle)
                .UseSchemaCache(searchDomain.UseSchemaCache)
                .UsingSSL(searchDomain.UseSSL)
                .Build();
        }

        //[Obsolete($"Use the overload with {nameof(ConnectionOptions)}.")]
        //public LdapConnection GetConnection(string? domain = null)
        //{
        //    if (string.IsNullOrWhiteSpace(domain) || !this.Domains.TryGetValue(domain, out SearchDomain? searchDomain))
        //        return this.GetDefaultConnection();

        //    return GetConnection(searchDomain, null);
        //}

        //[Obsolete($"Use the overload with {nameof(ConnectionOptions)}.")]
        //public LdapConnection GetConnection(string? domain, string? searchBase)
        //{
        //    if (string.IsNullOrWhiteSpace(domain) || !this.Domains.TryGetValue(domain, out SearchDomain? searchDomain))
        //        return this.GetDefaultConnection(searchBase);

        //    return GetConnection(searchDomain, searchBase);
        //}

        //[Obsolete($"Use the overload with {nameof(ConnectionOptions)}.")]
        //private static LdapConnection GetConnection(SearchDomain defDom, string? searchBase)
        //{
        //    string host = !string.IsNullOrWhiteSpace(defDom.StaticDomainController)
        //        ? defDom.StaticDomainController
        //        : defDom.FQDN;

        //    if (string.IsNullOrWhiteSpace(searchBase))
        //        searchBase = defDom.DistinguishedName;

        //    return new LdapConnectionBuilder()
        //        .UsingHost(host)
        //        .UsingSearchBase(searchBase)
        //        .UsingGlobalCatalog(defDom.UseGlobalCatalog)
        //        .UsingForestControls(defDom.IsForestRoot)
        //        .UseSchemaCache(defDom.UseSchemaCache)
        //        .UsingSSL(defDom.UseSSL)
        //        .Build();
        //}
    }
}
