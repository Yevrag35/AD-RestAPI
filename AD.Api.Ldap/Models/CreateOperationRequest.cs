using AD.Api.Ldap.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

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
        [LdapProperty("cn")]
        public string CommonName { get; set; } = string.Empty;

        public string? Path { get; set; }

        [JsonIgnore]
        public virtual CreationType Type { get; internal set; } = CreationType.Generic;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this.AddToProperties();
        }

        protected virtual void AddToProperties()
        {
            foreach (MemberInfo memInfo in this.GetAllLdapProperties())
            {
                var att = memInfo.GetCustomAttribute<LdapPropertyAttribute>() ?? throw new InvalidOperationException("huh?");
                if (string.IsNullOrWhiteSpace(att.LdapName) || !this.TryGetValue(memInfo, out object? value))
                    continue;

                this.Properties.Add(att.LdapName, JToken.FromObject(value));
            }
        }

        private IEnumerable<MemberInfo> GetAllLdapProperties()
        {
            return this.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.CustomAttributes.Any(x => x.AttributeType.IsAssignableTo(typeof(LdapPropertyAttribute))));
        }

        protected bool TryGetValue(MemberInfo memberInfo, [NotNullWhen(true)] out object? value)
        {
            value = null;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    value = ((FieldInfo)memberInfo).GetValue(this);
                    break;

                case MemberTypes.Property:
                    value = ((PropertyInfo)memberInfo).GetValue(this);
                    break;

                default:
                    return false;
            }

            return value is not null;
        }
    }
}
