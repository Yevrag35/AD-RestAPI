using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using AD.Api.Attributes;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Models
{
    [SupportedOSPlatform("windows")]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class UserQuery : JsonRequestBase
    {
        private readonly StringBuilder _builder = new StringBuilder(43);
        private bool _compiled;
        private static readonly Func<IDictionary<string, JToken>, char> _getOperator =
            x =>
                null != x
                &&
                x.ContainsKey(Strings.LDAPQuery_Operator)
                    ? GetOperatorChar(x[Strings.LDAPQuery_Operator].ToObject<string>())
                    : AMP;

        private char _operator;
        private string _cn;
        private string _email;
        private string _given;
        private string _sam;
        private string _surname;
        private string _upn;

        [JsonIgnore]
        public string Operator
        {
            get => _operator.ToString();
            set
            {
                char c = GetOperatorChar(value);
                this.SetOperator(c);
            }
        }

        [Ldap("cn")]
        [JsonProperty("cn")]
        public string CommonName
        {
            get => _cn;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _builder.Capacity = _builder.Length + 5 + value.Length;

                _cn = value;
            }
        }

        [Ldap("mail")]
        [JsonProperty("mail")]
        public string EmailAddress
        {
            get => _email;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _builder.Capacity = _builder.Length + 7 + value.Length;

                _email = value;
            }
        }

        [Ldap("givenname")]
        [JsonProperty("givenName")]
        public string GivenName
        {
            get => _given;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _builder.Capacity = _builder.Length + 12 + value.Length;

                _given = value;
            }
        }

        [Ldap("sn")]
        [JsonProperty("surname")]
        public string Surname
        {
            get => _surname;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _builder.Capacity = _builder.Length + 5 + value.Length;

                _surname = value;
            }
        }

        [Ldap("samaccountname")]
        [JsonProperty("samAccountName")]
        public string SamAccountName
        {
            get => _sam;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _builder.Capacity = _builder.Length + 17 + value.Length;

                _sam = value;
            }
        }

        [Ldap("userprincipalname")]
        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName
        {
            get => _upn;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _builder.Capacity = _builder.Length + 20 + value.Length;

                _upn = value;
            }
        }

        public async Task<string> AsFilterAsync()
        {
            return await Task.Run(() =>
            {
                if (_compiled)
                    return _builder.ToString();

                (PropertyInfo, string)[] nonNulls = this.GetNonNullProperties();

                for (int i = 0; i < nonNulls.Length; i++)
                {
                    (PropertyInfo, string) pi = nonNulls[i];
                    string value = pi.Item1.GetValue(this) as string;

                    _builder.Append(OP)
                                .Append(pi.Item2)
                                .Append(EQ)
                                .Append(value)
                            .Append(CP);
                }

                _builder.Append(CP);
                _compiled = true;

                return _builder.ToString();
            });
        }

        private static char GetOperatorChar(string operatorStr)
        {
            return Strings.LDAPQuery_Or.Equals(operatorStr, StringComparison.CurrentCultureIgnoreCase)
                ? PIP
                : AMP;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _builder.Append("(&(objectClass=user)(objectCategory=person)");
            char _op = _getOperator(this.ExtraData);
            this.SetOperator(_op);
        }

        private void SetOperator(char c)
        {
            if (PIP.Equals(c))
            {
                _builder.Replace(AMP, PIP, 1, 1);
            }

            _operator = c;
        }
    }
}
