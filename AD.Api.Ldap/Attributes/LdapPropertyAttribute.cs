using System;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class LdapPropertyAttribute : Attribute
    {
        public string? LdapName { get; }
        internal bool WantsLast => this.Index < 0;
        public int Index { get; }

        //public LdapPropertyAttribute(string? ldapName = null, string? displayName = null, string? shortHandName = null)
        public LdapPropertyAttribute(string? ldapName = null, int indexToGrab = -1)
        {
            this.LdapName = ldapName;
            this.Index = indexToGrab;

            //this.ShortName = shortHandName;
            //this.DisplayName = displayName;
        }
    }
}
