using System;
using System.Diagnostics;

namespace AD.Api.Attributes.Ldap
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class LdapConverterAttribute : Attribute, IValuedAttribute<Type>
    {
        public Type ConverterType { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type IValuedAttribute<Type>.Value => this.ConverterType;

        public LdapConverterAttribute(Type typeOfConverter)
        {
            this.ConverterType = typeOfConverter;
        }
    }
}
