using System;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class LdapIgnoreAttribute : Attribute
    {
    }
}
