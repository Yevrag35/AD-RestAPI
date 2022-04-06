using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Models
{
    public interface IJsonSerializable
    {
        void PrepareForSerialization(JsonSerializerSettings settings);
    }
}
