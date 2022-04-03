using System;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    public interface IFilterStatement
    {
        StringBuilder WriteTo(StringBuilder builder);
    }
}
