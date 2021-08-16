using Newtonsoft.Json;
using System;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Exceptions;
using AD.Api.Extensions;
using AD.Api.Models.Collections;
using AD.Api.Models.Converters;

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

        [Ldap("proxyaddresses")]
        [JsonProperty("proxyAddresses", Order = 7)]
        [JsonConverter(typeof(PropertyMethodConverter<string, ADSortedValueList<string>>))]
        public PropertyMethod<string> ProxyAddresses { get; set; }

        [Ldap("sn")]
        [JsonProperty("surname", Order = 6)]
        public string Surname { get; set; }

        [Ldap("samaccountname")]
        [JsonProperty("samAccountName", Order = 2)]
        public string SamAccountName { get; set; }

        [Ldap("userprincipalname")]
        [JsonProperty("userPrincipalName", Order = 3)]
        public string UserPrincipalName { get; set; }

        public bool IsRequestValid(out IllegalADOperationException e)
        {
            e = null;
            if (this.ProxyAddresses.IsInvalid)
                e = this.ProxyAddresses.GetException();

            return null == e;
        }

        public DirectoryEntry GetDirectoryEntry(string domainController = null)
        {
            if (string.IsNullOrWhiteSpace(this.DistinguishedName))
                throw new InvalidOperationException("DistinguishedName cannot be null or empty.");

            string ldapPath = this.DistinguishedName.ToLdapPath(domainController);
            return new DirectoryEntry(ldapPath);
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if (null == this.ProxyAddresses)
                this.ProxyAddresses = new PropertyMethod<string>();
        }
    }
}
