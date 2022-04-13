using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class CreateOperationRequest
    {
        [JsonExtensionData]
        public IDictionary<string, JToken?> Properties { get; private set; } = new SortedDictionary<string, JToken?>(StringComparer.CurrentCultureIgnoreCase);

        [JsonIgnore]
        public string? Domain { get; set; }

        [JsonProperty("cn", Required = Required.Always)]
        public string CommonName { get; set; } = string.Empty;

        public string? Path { get; set; }

        [JsonIgnore]
        public virtual CreationType Type { get; internal set; } = CreationType.Generic;
    }
}
