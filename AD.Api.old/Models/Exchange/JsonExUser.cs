using Newtonsoft.Json;
using System;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Components.Exchange;
using AD.Api.Exceptions;
using AD.Api.Extensions;
using AD.Api.Models.Collections;
using AD.Api.Models.Converters;

namespace AD.Api.Models.Exchange
{
    public class JsonExUser : JsonUser
    {
        [Ldap("msexchrecipientdisplaytype")]
        [JsonProperty("recipientDisplayType")]
        public RecipientDisplayType? RecipientDisplayType { get; set; }

        [Ldap("msexchrecipienttypedetails")]
        [JsonProperty("recipientTypeDetails")]
        public RecipientTypeDetails? RecipientTypeDetails { get; set; }

        [Ldap("msexchremoterecipienttype")]
        [JsonProperty("remoteRecipientType")]
        public RemoteRecipientType? RemoteRecipientType { get; set; }

        [Ldap("targetaddress")]
        [JsonProperty("targetAddress")]
        public string TargetAddress { get; set; }
    }
}
