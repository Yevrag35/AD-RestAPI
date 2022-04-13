using Newtonsoft.Json;
using System;

namespace AD.Api.Ldap.Models
{
    public interface IJsonPreparable
    {
        void PrepareForSerialization(JsonSerializerSettings settings);
    }
}
