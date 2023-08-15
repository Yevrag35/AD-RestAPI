using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PasswordRequest
    {
        [JsonExtensionData]
        private readonly IDictionary<string, JToken?> _extData = new Dictionary<string, JToken?>(StringComparer.CurrentCultureIgnoreCase);

        [JsonProperty("current")]   // Only needed if requesting a Password Change.
        public string? CurrentPassword { get; set; }

        [JsonProperty("dn", Required = Required.Always)]
        public string DistinguishedName { get; set; } = string.Empty;

        [JsonProperty("new")]
        public string NewPassword { get; set; } = string.Empty;

        [JsonProperty("encrypted")]
        public bool IsEncrypted { get; set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (string.IsNullOrWhiteSpace(this.NewPassword))
            {
                if (_extData.TryGetValue("password", out JToken? token) && token?.Type == JTokenType.String)
                {
                    this.NewPassword = token.ToObject<string>() ?? throw new ArgumentException($"{nameof(this.NewPassword)} must always be set in Password Requests.");
                }
                else if (_extData.TryGetValue("pass", out JToken? token2) && token2?.Type == JTokenType.String)
                {
                    this.NewPassword = token2.ToObject<string>() ?? throw new ArgumentException($"{nameof(this.NewPassword)} must always be set in Password Requests.");
                }
                else
                {
                    throw new ArgumentException($"{nameof(this.NewPassword)} must always be set in Password Requests.");
                }
            }
        }
    }
}
