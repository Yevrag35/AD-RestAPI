using System;

namespace AD.Api.Ldap.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class LdapConverterAttribute : Attribute
    {
        public Type ConverterType { get; }

        public LdapConverterAttribute(Type typeOfConverter)
        {
            this.ConverterType = typeOfConverter;
        }
    }
}
