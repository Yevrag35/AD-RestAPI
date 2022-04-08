using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LdapExtensionDataAttribute : Attribute
    {
        public bool IncludeComObjects { get; }

        public LdapExtensionDataAttribute()
        {
        }

        public LdapExtensionDataAttribute(bool includeCOMObjects)
        {
            this.IncludeComObjects = includeCOMObjects;
        }
    }
}
