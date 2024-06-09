using AD.Api.Ldap.Converters;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Path;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace AD.Api.Ldap.Models
{
    public class LdapUser : IPathed
    {
        [LdapExtensionData]
        [JsonIgnore]
        public IDictionary<string, object[]?>? ExtData { get; private set; }

        [JsonExtensionData]
        private IDictionary<string, JToken?>? _extData;

        [LdapProperty(ldapName: "mail")]
        [JsonProperty("mail")]
        public string? EmailAddress { get; set; }

        [LdapProperty("name")]
        public string? Name { get; set; }

        [LdapProperty("objectClass")]
        public string? ObjectClass { get; set; }

        [LdapIgnore]
        [JsonConverter(typeof(PathValueJsonConverter))]
        public PathValue? Path { get; set; }

        [LdapProperty("proxyAddresses")]
        public string[]? ProxyAddresses { get; set; }

        [LdapProperty("userPrincipalName")]
        public string? UserPrincipalName { get; set; }

        public LdapUser()
        {
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if (this.ExtData is null)
            {
                return;
            }
            else if (_extData is null)
            {
                _extData = new SortedDictionary<string, JToken?>(StringComparer.CurrentCultureIgnoreCase);
            }

            else
            {
                _extData.Clear();
            }

            foreach (KeyValuePair<string, object[]?> kvp in this.ExtData)
            {
                if (kvp.Value is not null && !kvp.Value.Any(o => o is MarshalByRefObject))
                {
                    _extData.Add(kvp.Key, JToken.FromObject(kvp.Value));
                }
            }
        }

#if DEBUG
        [Obsolete("Only for testing")]
        public Equal GetFilter<T>(string propertyName) where T : IConvertible
        {
            var exp = Expression.Parameter(typeof(LdapUser));
            var param = Expression.Property(exp, propertyName);

            Expression<Func<LdapUser, T?>> e = (Expression<Func<LdapUser, T?>>)Expression.Lambda(param, exp);

            return Equal.Create(this, e);
        }
#endif

        public Equal GetFilter<TMember>(Expression<Func<LdapUser, TMember?>> expression) where TMember : IConvertible
        {
            return Equal.Create(this, expression);
        }
        public static Equal GetFilter<TMember>(IConvertible? value, Expression<Func<LdapUser, TMember?>> expression)
        {
            return Equal.CreateWithValue(value, expression);
        }
    }
}
