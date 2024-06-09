using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// The <see langword="abstract"/> base record for LDAP filter statements.
    /// </summary>
#if OLDCODE
    public abstract class FilterStatementBase : IFilterStatement
#else
    public abstract record FilterStatementBase : IFilterStatement
#endif
    {
        public abstract int Length { get; }

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
        public virtual bool Equals(IFilterStatement? other)
        {
            return this.Type == other?.Type;
        }

        public abstract void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer);
        public virtual StringBuilder WriteTo(StringBuilder builder)
        {
            builder.EnsureCapacity(this.Length);
            return builder;
        }
    }
}
