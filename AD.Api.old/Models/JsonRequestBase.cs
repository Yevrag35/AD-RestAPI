using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using AD.Api.Attributes;
using AD.Api.Models.Entries;

namespace AD.Api.Models
{
    [SupportedOSPlatform("windows")]
    public abstract class JsonRequestBase
    {
        protected const char OP = (char)40;
        protected const char CP = (char)41;
        protected const char AMP = (char)38;
        protected const char PIP = (char)124;
        protected const char EQ = (char)61;
        private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;

        [JsonExtensionData]
        protected IDictionary<string, JToken> ExtraData;

        public IReadOnlyList<(string, object)> GetValues()
        {
            (PropertyInfo, string)[] pis = this.GetNonNullProperties();
            (string, object)[] values = new (string, object)[pis.Length];

            for (int i = 0; i < pis.Length; i++)
            {
                (PropertyInfo, string) item = pis[i];
                values[i] = (item.Item2, item.Item1.GetValue(this));
            }

            return values;
        }

        public (PropertyInfo, string)[] GetNonNullProperties()
        {
            PropertyInfo[] props = this.GetType().GetProperties(FLAGS);
            return props.Where(x =>
                x.GetCustomAttributes<LdapAttribute>().Any()
                &&
                null != x.GetValue(this))
                .Select(x =>
                    (x, x.GetCustomAttribute<LdapAttribute>().Name))
                .ToArray();
        }
    }
}
