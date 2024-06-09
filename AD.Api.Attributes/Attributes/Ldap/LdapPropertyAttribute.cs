using System;

namespace AD.Api.Attributes.Ldap
{
    /// <summary>
    /// Decorates a property or field and indicates that the specified LDAP property should be mapped to this member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class LdapPropertyAttribute : Attribute
    {
        public string LdapName { get; }
        public bool WantsLast { get; }
        public int Index { get; }

        public LdapPropertyAttribute(string ldapName = "", int indexToGrab = -1)
        {
            this.LdapName = ldapName;
            this.Index = indexToGrab;
            this.WantsLast = indexToGrab < 0;
        }
    }
}
