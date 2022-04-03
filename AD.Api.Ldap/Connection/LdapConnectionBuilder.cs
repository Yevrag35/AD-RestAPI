using AD.Api.Ldap.Connection;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap
{
    public class LdapConnectionBuilder
    {
        private readonly LdapConnectionOptions _options;

        public LdapConnectionBuilder()
        {
            _options = new LdapConnectionOptions();
        }

        public LdapConnection Build()
        {
            return new LdapConnection(_options);
        }

        public LdapConnectionBuilder UsingAuthenticationTypes(AuthenticationTypes authenticationTypes)
        {
            _options.AuthenticationTypes = authenticationTypes;
            return this;
        }

        public LdapConnectionBuilder UsingCredentials(NetworkCredential? credential)
        {
            _options.Credential = credential;
            return this;
        }

        public LdapConnectionBuilder UsingGlobalCatalog(bool toggle = true)
        {
            _options.Protocol = toggle ? Protocol.GlobalCatalog : Protocol.Ldap;
            return this;
        }

        public LdapConnectionBuilder UsingHost(string? domainOrDc)
        {
            _options.Host = domainOrDc;
            return this;
        }

        public LdapConnectionBuilder UsingSearchBase(string? searchBase)
        {
            _options.DistinguishedName = searchBase;
            return this;
        }

        public LdapConnectionBuilder UsingSSL(bool toggle = true)
        {
            _options.UseSSL = toggle;
            return this;
        }

        private record LdapConnectionOptions : ILdapConnectionOptions
        {
            public AuthenticationTypes? AuthenticationTypes { get; internal set; }
            public NetworkCredential? Credential { get; internal set; }
            public string? DistinguishedName { get; internal set; }
            public string? Host { get; internal set; }
            public Protocol Protocol { get; internal set; }
            public bool UseSSL { get; internal set; }

            internal LdapConnectionOptions()
            {
            }

            public NetworkCredential? GetCredential()
            {
                return this.Credential;
            }
        }
    }
}
