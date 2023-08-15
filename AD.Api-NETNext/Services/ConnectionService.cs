using AD.Api.Domains;
using AD.Api.Ldap;
using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Path;
using AD.Api.Schema;
using Microsoft.Win32.SafeHandles;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface IConnectionService
    {
        LdapConnection GetConnection(Action<ConnectionOptions> configureOptions);
        LdapConnection GetConnection(IConnectionOptions options);
    }

    public class ConnectionService : IConnectionService
    {
        private readonly ILdapEnumDictionary _enumDictionary;

        private SearchDomains Domains { get; }
        private IIdentityService Identity { get; }

        public ConnectionService(IIdentityService identityService, SearchDomains searchDomains, ILdapEnumDictionary enumDictionary)
        {
            _enumDictionary = enumDictionary;
            this.Identity = identityService;
            this.Domains = searchDomains;
        }

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

            return new LdapConnectionBuilder(_enumDictionary)
                .UsingHost(host)
                .UsingSearchBase(searchBase)
                .UsingGlobalCatalog(searchDomain.UseGlobalCatalog)
                .UsingAuthenticationTypes(System.DirectoryServices.AuthenticationTypes.Secure)
                .UsingForestControls(searchDomain.IsForestRoot)
                .UsingIdentity(token, options.DontDisposeHandle)
                .UseSchemaCache(searchDomain.UseSchemaCache)
                .UsingSSL(searchDomain.UseSSL)
                .Build();
        }
    }
}
