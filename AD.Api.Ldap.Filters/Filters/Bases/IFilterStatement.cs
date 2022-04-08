using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// Provides logic for implementing types to define LDAP filter syntax.
    /// </summary>
    public interface IFilterStatement : IEquatable<IFilterStatement>
    {
        /// <summary>
        /// The filter keyword of the statement or <see cref="FilterType.Equal"/> is no keyword matches.
        /// </summary>
        FilterType Type { get; }

        /// <summary>
        /// Serializes the <see cref="IFilterStatement"/> into JSON and writes it to the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">The writer to write the JSON to.</param>
        /// <param name="strategy">The naming strategy for property names.</param>
        /// <param name="serializer">The serializer used when serializing values.</param>
        void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer);

        /// <summary>
        /// Serializes the <see cref="IFilterStatement"/> into standard LDAP filter syntax.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to write the LDAP filter to.</param>
        /// <returns>
        ///     The passed instance of <see cref="StringBuilder"/> with the LDAP filter written.
        /// </returns>
        StringBuilder WriteTo(StringBuilder builder);
    }
}
