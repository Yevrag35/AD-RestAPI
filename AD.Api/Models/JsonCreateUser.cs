using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Extensions;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class JsonCreateUser : JsonRequestBase
    {
        [Ldap("cn")]
        [JsonProperty("cn", Order = 1)]
        public string CommonName { get; set; }

        [Ldap("displayname")]
        [JsonProperty("displayName", Order = 2)]
        public string DisplayName { get; set; }

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

        [Ldap("sn")]
        [JsonProperty("surname", Order = 7)]
        public string Surname { get; set; }

        public bool UseDefaultOU()
        {
            return string.IsNullOrWhiteSpace(this.OUPath);
        }

        private bool ShouldSerializeOUPath() => false;
    }
}
