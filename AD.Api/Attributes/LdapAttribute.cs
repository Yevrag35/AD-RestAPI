using Linq2Ldap.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AD.Api.Attributes
{
    [SupportedOSPlatform("windows")]
    public class LdapAttribute : LdapFieldAttribute
    {
        public LdapAttribute(string name, bool isMandatory = false)
            : base(name, !isMandatory)
        {
        }
    }
}
