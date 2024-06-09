namespace AD.Api.Startup.Services
{
    /// <summary>
    /// An interface for querying the types that have been specified for exclusion from service registration.
    /// </summary>
    public interface IServiceTypeExclusions : IReadOnlyCollection<Type>
    {
        /// <summary>
        /// Returns whether the exact type is contained within this exclusion set.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns>
        ///     <see langword="true"/> if the type is contained; otherwise, <see langword="false"/>.
        /// </returns>
        bool Contains<T>();
        /// <summary>
        /// Returns whether the exact type is contained within this exclusion set.
        /// </summary>
        /// <param name="type">The type or type definition to check.</param>
        /// <returns>
        ///     <see langword="true"/> if the type or type definition is contained; otherwise, <see langword="false"/>.
        /// </returns>
        bool Contains(Type type);
        /// <summary>
        /// Returns whether the specified type should be excluded from service registration.
        /// </summary>
        /// <remarks>
        ///     If <paramref name="type"/> is a generic type, then the generic type definition will be also be checked
        ///     to see if it is excluded.
        /// </remarks>
        /// <param name="type">The type or type definition to check.</param>
        /// <returns>
        ///     <see langword="true"/> if the type should be excluded; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsExcluded(Type type);
    }
}

