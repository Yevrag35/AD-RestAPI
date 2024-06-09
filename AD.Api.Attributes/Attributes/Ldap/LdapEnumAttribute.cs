using System;
using System.Diagnostics;

namespace AD.Api.Attributes.Ldap
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public sealed class LdapEnumAttribute : AdApiAttribute, IValuedAttribute<string>
    {
        public string Name { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string IValuedAttribute<string>.Value => this.Name;

        public LdapEnumAttribute(string LdapName = "")
        {
            this.Name = LdapName;
        }
    }
}
