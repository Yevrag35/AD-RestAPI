using System;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class LdapPropertyAttribute : Attribute
    {
        public string? LdapName { get; }

        //public LdapPropertyAttribute(string? ldapName = null, string? displayName = null, string? shortHandName = null)
        public LdapPropertyAttribute(string? ldapName = null)
        {
            this.LdapName = ldapName;
            //this.ShortName = shortHandName;
            //this.DisplayName = displayName;
        }
    }
}
