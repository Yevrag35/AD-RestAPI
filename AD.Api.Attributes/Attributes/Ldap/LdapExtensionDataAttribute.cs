using System;

namespace AD.Api.Attributes.Ldap
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class LdapExtensionDataAttribute : Attribute
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
