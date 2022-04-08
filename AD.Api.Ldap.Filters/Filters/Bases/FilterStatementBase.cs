using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// The <see langword="abstract"/> base class for LDAP filter statements.
    /// </summary>
    public abstract class FilterStatementBase : IFilterStatement
    {
        public abstract FilterType Type { get; }

        /// <summary>
        /// Determines if the current <see cref="FilterStatementBase"/> equals the specified <see cref="IFilterStatement"/> 
        /// implementation.
        /// </summary>
        /// <remarks>
        ///     By default, <see cref="FilterStatementBase"/> simply checks if <paramref name="other"/> is not <see langword="null"/>
        ///     and whether <see cref="Type"/> is equal to <see cref="IFilterStatement.Type"/>.
        /// </remarks>
        /// <param name="other">The implementation to check equality against.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="other"/> is equal to the current instance of <see cref="FilterStatementBase"/>;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool Equals(IFilterStatement other)
        {
            return !(other is null) && this.Type == other.Type;
        }

        public abstract void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer);
        public abstract StringBuilder WriteTo(StringBuilder builder);
    }
}
