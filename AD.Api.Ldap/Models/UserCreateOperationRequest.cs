using AD.Api.Ldap.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class UserCreateOperationRequest : CreateOperationRequest
    {
        [JsonProperty("userAccountControl")]
        private string? _userAccountControl;

        [JsonIgnore]
        public override CreationType Type { get; internal set; } = CreationType.User;

        [JsonIgnore]
        [LdapProperty("userAccountControl")]
        public UserAccountControl? UserAccountControl => Enum.TryParse(_userAccountControl, out UserAccountControl uac)
            ? uac
            : null;

        //[OnDeserialized]
        //private void OnDeserialized(StreamingContext ctx)
        //{
        //    if (this.UserAccountControl.HasValue && !this.Properties.ContainsKey(nameof(this.UserAccountControl)))
        //    {
        //        this.Properties.Add(nameof(this.UserAccountControl), new JValue((int)this.UserAccountControl.Value));
        //    }
        //}
    }
}
