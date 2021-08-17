using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using AD.Api.Attributes;
using AD.Api.Components;
using AD.Api.Extensions;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Models
{
    [SupportedOSPlatform("windows")]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class JsonCreateUser : JsonRequestBase, IDirObject
    {
        [Ldap("cn")]
        [JsonProperty("cn", Order = 1)]
        public string CommonName { get; set; }

        [Ldap("displayname")]
        [JsonProperty("displayName", Order = 2)]
        public string DisplayName { get; set; }

        [JsonIgnore]
        string IDirObject.DistinguishedName => this.OUPath;

        [Ldap("mail")]
        [JsonProperty("mail", Order = 3)]
        public string EmailAddress { get; set; }

        [Ldap("givenname")]
        [JsonProperty("givenName", Order = 4)]
        public string GivenName { get; set; }

        [Ldap("name")]
        [JsonProperty("name", Order = 5)]
        public string Name { get; set; }

        [JsonProperty("ouPath", Order = 6)]
        public string OUPath { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [Ldap("sn")]
        [JsonProperty("surname", Order = 7)]
        public string Surname { get; set; }

        //public string GetCommonName()
        //{
        //    if (!string.IsNullOrWhiteSpace(this.CommonName))
        //        return FormatName(this.CommonName);

        //    else if (!string.IsNullOrWhiteSpace(this.Name))
        //        return FormatName(this.Name);

        //    else if (!string.IsNullOrWhiteSpace(this.GivenName))
        //}
        public DirectoryEntry GetDirectoryEntry(string domainController)
        {
            return new DirectoryEntry(this.OUPath.ToLdapPath());
        }
        public DirectoryEntry GetDirectoryEntry(string domainController, string domain)
        {
            if (!string.IsNullOrWhiteSpace(this.OUPath))
                return this.GetDirectoryEntry(domainController);

            else if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentNullException(nameof(domain));

            string guid = AttributeReader.GetAdditionalValue<string>(WellKnownObjects.Users);
            this.OUPath = Strings.LDAP_Format_WKO.Format(guid, domain);

            return this.GetDirectoryEntry(domainController);
        }
        private static string FormatName(string name)
        {
            return Strings.CommonName_Format.Format(
                Regex.Replace(
                    name, Strings.Escape_Commas, Strings.Escape_Commas
                )
            );
        }
        public bool UseDefaultOU()
        {
            return string.IsNullOrWhiteSpace(this.OUPath);
        }

        private bool ShouldSerializeOUPath() => false;
    }
}
