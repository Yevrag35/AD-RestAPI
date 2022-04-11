using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Operations
{
    public enum OperationType
    {
        Commit,
        Set,
        Add,
        Remove,
        Replace
    }
}
