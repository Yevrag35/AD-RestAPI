using AD.Api.Ldap.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace AD.Api.Ldap.Converters
{
    /// <summary>
    /// The converter that transforms LDAP filter syntax to and from JSON.
    /// </summary>
    public partial class FilterConverter : JsonConverter<IFilterStatement>
    {
        private NamingStrategy NamingStrategy { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="FilterConverter"/> using the optionally specified 
        /// <see cref="Newtonsoft.Json.Serialization.NamingStrategy"/>
        /// for serializing property names.
        /// </summary>
        /// <remarks>
        ///     When <paramref name="namingStrategy"/> is <see langword="null"/>, 
        ///     <see cref="CamelCaseNamingStrategy"/> will be used.
        /// </remarks>
        /// <param name="namingStrategy">The optional naming strategy.</param>
        public FilterConverter(NamingStrategy? namingStrategy = null)
        {
            if (namingStrategy is null)
                namingStrategy = new CamelCaseNamingStrategy();

            this.NamingStrategy = namingStrategy;
        }
    }
}
