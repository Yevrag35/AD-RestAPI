using System;

namespace AD.Api.Ldap.Attributes
{
    /// <summary>
    /// Decorates a property or field and indicates that the specified LDAP property should be mapped to this member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class LdapPropertyAttribute : Attribute
    {
        public string? LdapName { get; }
        public bool WantsLast => this.Index < 0;
        public int Index { get; }

        public LdapPropertyAttribute(string? ldapName = null, int indexToGrab = -1)
        {
            this.LdapName = ldapName;
            this.Index = indexToGrab;
        }
    }
}
