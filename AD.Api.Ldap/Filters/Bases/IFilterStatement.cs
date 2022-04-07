using System;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    public interface IFilterStatement : IEquatable<IFilterStatement>
    {
        FilterType Type { get; }
        StringBuilder WriteTo(StringBuilder builder);
    }
}
