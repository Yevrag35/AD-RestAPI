using Newtonsoft.Json;

namespace AD.Api.Ldap.Models
{
    public interface IJsonPreparable
    {
        void PrepareForSerialization(JsonSerializerSettings settings);
    }
}
