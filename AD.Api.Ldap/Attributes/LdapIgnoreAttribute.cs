using System;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LdapIgnoreAttribute : Attribute
    {
    }
}
