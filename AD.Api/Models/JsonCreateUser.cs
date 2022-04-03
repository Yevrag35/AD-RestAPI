using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using AD.Api.Attributes;
using AD.Api.Components;
using AD.Api.Extensions;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Models
{
    [SupportedOSPlatform("windows")]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class JsonCreateUser : JsonRequestBase, IDirObject
    {
        [Ldap("cn")]
        [JsonProperty("cn", Order = 1)]
        public string CommonName { get; set; }

        [Ldap("displayname")]
        [JsonProperty("displayName", Order = 2)]
        public string DisplayName { get; set; }

        [JsonIgnore]
        string IDirObject.DistinguishedName => this.OUPath;

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

        [JsonProperty("password")]
        public string Password { get; set; }

        [Ldap("sn")]
        [JsonProperty("surname", Order = 7)]
        public string Surname { get; set; }

        public DirectoryEntry GetDirectoryEntry(string domainController)
        {
            return new DirectoryEntry(this.OUPath.ToLdapPath());
        }
        public DirectoryEntry GetDirectoryEntry(string domainController, string domain)
        {
            if (!string.IsNullOrWhiteSpace(this.OUPath))
                return this.GetDirectoryEntry(domainController);

            else if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentNullException(nameof(domain));

            string guid = AttributeReader.GetAdditionalValue<string>(WellKnownObjects.Users);
            this.OUPath = Strings.LDAP_Format_WKO.Format(guid, domain);

            return this.GetDirectoryEntry(domainController);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (null != this.ExtraData && this.ExtraData.Count > 0)
                this.ExtraData = new Dictionary<string, JToken>(this.ExtraData, StringComparer.CurrentCultureIgnoreCase);

            else
                return;

            this.CheckValue(x => x.CommonName, "commonName");
            this.CheckValue(x => x.EmailAddress, "email", "emailAddress");
            this.CheckValue(x => x.GivenName, "firstName");
            this.CheckValue(x => x.OUPath, "ou");
            this.CheckValue(x => x.Surname, "sn", "lastName");
        }
        private bool ShouldSerializeOUPath() => false;
        public bool UseDefaultOU()
        {
            return string.IsNullOrWhiteSpace(this.OUPath);
        }

        private void CheckValue<T>(Expression<Func<JsonCreateUser, T>> expression, params string[] additionalNames)
        {
            if (null == additionalNames || additionalNames.Length <= 0)
                return;

            PropertyInfo memInfo = null;
            if (expression.Body is MemberExpression memEx && memEx.Member.MemberType == MemberTypes.Property)
                memInfo = memEx.Member as PropertyInfo;

            else if (expression.Body is UnaryExpression unEx && unEx.Operand is MemberExpression unExMem)
                memInfo = unExMem.Member as PropertyInfo;

            if (null == memInfo)
                return;

            SetValuesIfTheyExist(
                additionalNames,
                this,
                expression.Compile(),
                (user, token) => memInfo.SetValue(user, token.ToObject<T>())
            );
        }

        private static void SetValuesIfTheyExist<T>(string[] names, JsonCreateUser user, Func<JsonCreateUser, T> function, 
            Action<JsonCreateUser, JToken> action)
        {
            if (null != function(user))
                return;

            for (int i = 0; i < names.Length; i++)
            {
                if (user.ExtraData.TryGetValue(names[i], out JToken token))
                {
                    action(user, token);
                    for (int i2 = 0; i2 < names.Length; i2++)
                    {
                        user.ExtraData.Remove(names[i2]);
                    }
                    
                    break;
                }
            }
        }
    }
}
