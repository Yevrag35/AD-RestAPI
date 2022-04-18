using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Connection;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap
{
    public class LdapConnectionBuilder
    {
        private readonly LdapConnectionOptions _options;
        private readonly ILdapEnumDictionary _enumDictionary;

        public LdapConnectionBuilder(ILdapEnumDictionary enumDictionary)
        {
            _options = new LdapConnectionOptions();
            _enumDictionary = enumDictionary;
        }

        public LdapConnection Build()
        {
            return new LdapConnection(_options, _enumDictionary);
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

        public LdapConnectionBuilder UsingForestControls(bool toggle = true)
        {
            _options.IsForest = toggle;
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

        public LdapConnectionBuilder UsingIdentity(SafeAccessTokenHandle? token, bool dontDisposeToken = true)
        {
            _options.Token = token;
            _options.DontDisposeToken = dontDisposeToken;
            return this;
        }

        public LdapConnectionBuilder UsingSearchBase(string? searchBase)
        {
            _options.DistinguishedName = searchBase;
            return this;
        }

        public LdapConnectionBuilder UseSchemaCache(bool toggle = true)
        {
            _options.UseSchemaCache = toggle;
            return this;
        }

        public LdapConnectionBuilder UsingSSL(bool toggle = true)
        {
            _options.UseSSL = toggle;
            return this;
        }

        private record LdapConnectionOptions : ILdapConnectionOptions
        {
            private bool _useSchemaCache;

            public AuthenticationTypes? AuthenticationTypes { get; internal set; }
            public NetworkCredential? Credential { get; internal set; }
            public string? DistinguishedName { get; internal set; }
            public bool DontDisposeToken { get; internal set; } = true;
            public bool IsForest { get; internal set; }
            public string? Host { get; internal set; }
            public Protocol Protocol { get; internal set; }
            public SafeAccessTokenHandle? Token { get; internal set; }
            public bool UseSchemaCache
            {
                get => _useSchemaCache && this.IsForest;
                set => _useSchemaCache = value;
            }
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
