using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Path;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace AD.Api.Ldap.Models
{
    public class LdapUser : IPathed
    {
        [LdapExtensionData]
        public IDictionary<string, object[]?>? ExtData;

        [LdapProperty("mail")]
        public string? EmailAddress { get; private set; }

        [LdapProperty]
        public string? Name { get; private set; }

        [LdapIgnore]
        public PathValue? Path { get; internal set; }

        [LdapProperty]
        public string[]? ProxyAddresses { get; private set; }

        [LdapProperty]
        public string? UserPrincipalName { get; private set; }

        public LdapUser()
        {
        }
    }
}
