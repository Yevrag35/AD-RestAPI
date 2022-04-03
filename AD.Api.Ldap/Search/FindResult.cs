using AD.Api.Ldap.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AD.Api.Ldap.Search
{
    public class FindResult
    {
        [JsonExtensionData]
        private IDictionary<string, JToken?>? _jsonData;

        [LdapExtensionData]
        [JsonIgnore]
        public IDictionary<string, object[]>? Data { get; private set; }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if (this.Data is null)
                return;

            if (_jsonData is null)
            {
                _jsonData = new SortedDictionary<string, JToken?>(StringComparer.CurrentCultureIgnoreCase);
            }

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

                _jsonData.Add(kvp.Key, token);
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext ctx)
        {
            _jsonData?.Clear();
        }
    }
}
