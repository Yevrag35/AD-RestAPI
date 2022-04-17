using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public sealed class LdapEnumAttribute : Attribute
    {
        public string? Name { get; }

        public LdapEnumAttribute(string? LdapName = null)
        {
            this.Name = LdapName;
        }
    }
}
