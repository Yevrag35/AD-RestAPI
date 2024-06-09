namespace AD.Api.Startup.Services
{
    /// <summary>
    /// An interface for adding services by type or type definition that should be excluded from being registered
    /// in the application's Dependency Injection container.
    /// </summary>
    /// <note>
    ///     Adding a type will only register that exact type and will not affect derived or inherited types. However,
    ///     registering type definitions will exclude all generic variations of the type.
    /// </note>
    public interface IAddServiceTypeExclusions
    {
        /// <summary>
        /// The total number of registered exclusion types whether they are explicit or generic definitions.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds the specified exact service type <typeparamref name="T"/> to be excluded from service registration.
        /// </summary>
        /// <remarks><inheritdoc cref="IAddServiceTypeExclusions" path="/note"/></remarks>
        /// <typeparam name="T">The type of service to exclude.</typeparam>
        /// <returns>This same <see cref="IAddServiceTypeExclusions"/> instance for chaining.</returns>
        IAddServiceTypeExclusions Add<T>();
        /// <summary>
        /// Adds the specified service type or type definition to be excluded from service registration.
        /// </summary>
        /// <remarks><inheritdoc cref="IAddServiceTypeExclusions" path="/note"/></remarks>
        /// <param name="type">
        ///     The type or type definition to exclude from service registration.
        /// </param>
        /// <returns>This same <see cref="IAddServiceTypeExclusions"/> instance for chaining.</returns>
        IAddServiceTypeExclusions Add(Type type);
    }
}

