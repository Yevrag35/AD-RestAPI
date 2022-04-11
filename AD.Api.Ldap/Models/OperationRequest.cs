using AD.Api.Ldap.Converters;
using AD.Api.Ldap.Path;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Operations
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class OperationRequest
    {
        [JsonProperty("dn", Order = 1, Required = Required.Always)]
        public string DistinguishedName { get; private set; } = string.Empty;

        [JsonProperty("edits", Order = 2)]
        public List<ILdapOperation> EditOperations { get; private set; } = new List<ILdapOperation>(1);
    }
}
