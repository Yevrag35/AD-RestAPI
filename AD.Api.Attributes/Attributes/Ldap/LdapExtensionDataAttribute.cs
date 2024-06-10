using System;

namespace AD.Api.Attributes.Ldap
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class LdapExtensionDataAttribute : AdApiAttribute
    {
        public bool IncludeComObjects { get; }

        public LdapExtensionDataAttribute()
        {
        }

        public LdapExtensionDataAttribute(bool includeCOMObjects)
        {
            this.IncludeComObjects = includeCOMObjects;
        }
    }
}