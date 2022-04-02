using System;
using System.Text;

namespace AD.Api.Ldap.Filter
{
    public interface IFilterStatement
    {
        StringBuilder Generate(StringBuilder builder);
    }
}
