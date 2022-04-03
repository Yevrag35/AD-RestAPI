using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Path
{
    public interface IPathBuilder
    {
        string DistinguishedName { get; set; }
        string Domain { get; }
        int Port { get; }
        string Protocol { get; }
        bool SSL { get; set; }

        string ToPath();
    }
}
