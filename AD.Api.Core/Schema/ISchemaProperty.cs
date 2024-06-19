using AD.Api.Core.Ldap;

namespace AD.Api.Core.Schema
{
    public interface ISchemaProperty
    {
        /// <summary>
        /// Indicates whether the property is an array of values rather than a single value.
        /// </summary>
        bool IsMultiValued { get; }
        /// <summary>
        /// The LDAP value type of the schema property. Used for value conversion.
        /// </summary>
        LdapValueType LdapType { get; }
        /// <summary>
        /// The name of the schema property.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The .NET runtime type the schema property's value(s) is/are mapped to.
        /// </summary>
        Type RuntimeType { get; }
    }
}

