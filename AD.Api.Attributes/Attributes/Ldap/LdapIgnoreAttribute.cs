using System;

namespace AD.Api.Attributes.Ldap
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class LdapIgnoreAttribute : AdApiAttribute
    {
    }
}
