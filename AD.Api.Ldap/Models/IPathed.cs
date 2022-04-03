using AD.Api.Ldap.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Models
{
    public interface IPathed
    {
        PathValue? Path { get; }
    }
}
