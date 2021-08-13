using Linq2Ldap.Core.Attributes;
using Linq2Ldap.Core.Models;
using Linq2Ldap.Core.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AD.Api.Attributes;
using AD.Api.Components;

namespace AD.Api.Models.Entries
{
    [SupportedOSPlatform("windows")]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class User : EntryBase<User>
    {
        [Ldap("cn")]
        [PropertyValue(AddMethod.Set)]
        [JsonProperty("cn")]
        public string CommonName { get; set; }

        [Ldap("mail")]
        [PropertyValue(AddMethod.Set)]
        [JsonProperty("email")]
        public string EmailAddress { get; set; }

        [Ldap("givenname")]
        [PropertyValue(AddMethod.Set)]
        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [Ldap("sn")]
        [PropertyValue(AddMethod.Set)]
        [JsonProperty("surname")]
        public string Surname { get; set; }

        [Ldap("samaccountname")]
        [PropertyValue(AddMethod.Set)]
        [JsonProperty("samAccountName")]
        public string SamAccountName { get; set; }

        [Ldap("userprincipalname")]
        [PropertyValue(AddMethod.Set)]
        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonConstructor]
        public User()
            : base()
        {
        }
    }
}
