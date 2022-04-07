using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public enum FilterType
    {
        Equal,
        And,
        Or,
        Not,
        Nor,
        Band,
        Bor
    }
}
