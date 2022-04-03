using System;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LdapPropertyAttribute : Attribute
    {
        public string? ShortName { get; }
        public string? DisplayName { get; }
        public string? LdapName { get; }

        public LdapPropertyAttribute(string? ldapName = null, string? displayName = null, string? shortHandName = null)
        {
            this.ShortName = shortHandName;
            this.DisplayName = displayName;
            this.LdapName = ldapName;
        }
    }
}
