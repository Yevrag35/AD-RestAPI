using System;

namespace AD.Api.Attributes.Ldap
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public sealed class LdapEnumAttribute : Attribute
    {
        public string Name { get; }

        public LdapEnumAttribute(string LdapName = "")
        {
            this.Name = LdapName;
        }
    }
}
