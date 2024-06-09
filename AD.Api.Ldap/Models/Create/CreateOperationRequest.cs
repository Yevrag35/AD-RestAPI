using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Claims;

namespace AD.Api.Ldap.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CreateOperationRequest
    {
        [JsonIgnore]
        public ClaimsPrincipal? ClaimsPrincipal { get; set; }

        /// <summary>
        /// The domain to create the account in.  Must be registered with the API.
        /// </summary>
        [JsonIgnore]
        public string? Domain { get; set; }

        /// <summary>
        /// The "CommonName" of the new account.
        /// </summary>
        /// <remarks>This value should NOT have the CN= prefix appended.</remarks>
        /// <example>John Doe</example>
        [JsonProperty("cn", Required = Required.Always)]
        [DisplayName("CN")]
        [Required]
        public string CommonName { get; set; } = string.Empty;

        /// <summary>
        /// The distinguished name of the OU or container to create the account in.
        /// </summary>
        /// <example>OU=AllUsers,DC=contoso,DC=com</example>
        [JsonProperty]
        [DefaultValue("<The default user container or OU>")]
        public string? Path { get; set; }

        /// <summary>
        /// Any additional attributes to set on the account after creation. This property is actually not specified in the body by "Properties", instead any additional properties in the body will
        /// be added into this value.
        /// </summary>
        [JsonExtensionData]
        [DefaultValue(null)]
        public IDictionary<string, JToken?> Properties { get; private set; } = new SortedDictionary<string, JToken?>(StringComparer.CurrentCultureIgnoreCase);

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
                {
                    continue;
                }

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
