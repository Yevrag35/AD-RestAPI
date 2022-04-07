using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    [Flags]
    public enum FilterType
    {
        Equal = 1,
        And = 2,
        Or = 4,
        Not = 8,
        Nor = 16,
        Band = 32,
        Bor = 64,
        Recurse = 128
    }
}
