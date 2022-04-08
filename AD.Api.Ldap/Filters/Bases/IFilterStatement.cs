using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    public interface IFilterStatement : IEquatable<IFilterStatement>
    {
        FilterType Type { get; }

        void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer);
        StringBuilder WriteTo(StringBuilder builder);
    }
}
