using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Converters;
using AD.Api.Ldap.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

namespace AD.Api.Ldap.Search
{
    public class FindResult : IJsonPreparable
    {
        [JsonExtensionData(ReadData = false, WriteData = true)]
        [LdapIgnore]
        protected IDictionary<string, JToken?> JsonData { get; } =
            new SortedDictionary<string, JToken?>(StringComparer.CurrentCultureIgnoreCase);

        [JsonIgnore]
        [LdapProperty]
        [LdapConverter(typeof(FileTimeConverter))]
        public DateTime? BadPasswordTime { get; private set; }

        [JsonIgnore]
        [LdapExtensionData]
        public IDictionary<string, object[]>? Data { get; private set; }

        [JsonIgnore]
        [LdapProperty]
        public GroupType? GroupType { get; private set; }

        [JsonIgnore]
        [LdapProperty]
        [LdapConverter(typeof(FileTimeConverter))]
        public DateTime? LastLogon { get; private set; } 

        [JsonIgnore]
        [LdapProperty]
        [LdapConverter(typeof(FileTimeConverter))]
        public DateTime? PwdLastSet { get; private set; }

        [JsonIgnore]
        [LdapProperty]
        public UserAccountControl? UserAccountControl { get; private set; }

        protected void AddExpressionsToJsonData<T>(T obj, JsonSerializerSettings settings, params Expression<Func<T, object?>>[] expressions)
            where T : FindResult
        {
            if (expressions is null || expressions.Length <= 0)
                return;

            Array.ForEach(expressions, exp =>
            {
                if (TrySerializeProperty(obj, settings, 
                    out KeyValuePair<string, JToken?>? kvp,
                    exp))
                {
                    this.JsonData.Add(kvp.Value);
                }
            });
        }

        public void PrepareForSerialization(JsonSerializerSettings settings)
        {
            if (this.Data is null)
                return;

            this.OnPreparing(settings);

            foreach (var kvp in this.Data)
            {
                JToken? token = null;
                switch (kvp.Value?.Length)
                {
                    case 1:
                        token = new JValue(kvp.Value[0]);
                        break;

                    case > 1:
                        token = JToken.FromObject(kvp.Value);
                        break;

                    default:
                        break;
                }

                JsonData.Add(kvp.Key, token);
            }

            this.Data.Clear();
        }

        protected virtual void OnPreparing(JsonSerializerSettings settings)
        {
            this.AddExpressionsToJsonData(this, settings,
                x => x.BadPasswordTime,
                x => x.GroupType,
                x => x.LastLogon,
                x => x.PwdLastSet,
                x => x.UserAccountControl);
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext ctx)
        {
            JsonData?.Clear();
        }

        protected static bool TryAsMemberExpression<T, TMember>([NotNullWhen(true)] out MemberExpression? member,
            Expression<Func<T, TMember>> expression)
        {
            member = null;
            if (expression?.Body is MemberExpression memEx)
            {
                member = memEx;
            }
            else if (expression?.Body is UnaryExpression unEx && unEx.Operand is MemberExpression unExMem)
            {
                member = unExMem;
            }

            return member != null;
        }

        protected static bool TrySerializeProperty<T>(T obj, JsonSerializerSettings settings, 
            [NotNullWhen(true)] out KeyValuePair<string, JToken?>? kvp,
            Expression<Func<T, object?>> expression)
        {
            kvp = null;
            if (!TryAsMemberExpression(out MemberExpression? memberExpression, expression))
                return false;

            Func<T, object?> func = expression.Compile();
            object? value = func(obj);

            if (value is null)
                return false;

            string jsonKey = memberExpression.Member.Name.ToLower();

            string rawJson = JsonConvert.SerializeObject(value, settings);
            JToken token = JToken.Parse(rawJson);

            kvp = new KeyValuePair<string, JToken?>(jsonKey, token);
            return true;
        }
    }
}
