using Newtonsoft.Json;
using System;
using System.DirectoryServices;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Extensions;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Models
{
    [SupportedOSPlatform("windows")]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class JsonUser : JsonRequestBase, IDirObject
    {
        private string _dn;

        [Ldap("distinguishedname")]
        [JsonProperty("dn", Order = 1)]
        public string DistinguishedName
        {
            get => _dn;
            set
            {
                if (!string.IsNullOrEmpty(value) 
                    && 
                    value.StartsWith(Strings.LDAP_Prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    _dn = value.Substring(Strings.LDAP_Prefix.Length);
                }
                else
                {
                    _dn = value;
                }
            }
        }

        [Ldap("mail")]
        [JsonProperty("mail", Order = 4)]
        public string EmailAddress { get; set; }

        [Ldap("givenname")]
        [JsonProperty("givenName", Order = 5)]
        public string GivenName { get; set; }

        [Ldap("sn")]
        [JsonProperty("surname", Order = 6)]
        public string Surname { get; set; }

        [Ldap("samaccountname")]
        [JsonProperty("samAccountName", Order = 2)]
        public string SamAccountName { get; set; }

        [Ldap("userprincipalname")]
        [JsonProperty("userPrincipalName", Order = 3)]
        public string UserPrincipalName { get; set; }

        public DirectoryEntry GetDirectoryEntry(string domainController = null)
        {
            if (string.IsNullOrWhiteSpace(this.DistinguishedName))
                throw new InvalidOperationException("DistinguishedName cannot be null or empty.");

            string ldapPath = this.DistinguishedName.ToLdapPath(domainController);
            return new DirectoryEntry(ldapPath);
        }
    }
}
