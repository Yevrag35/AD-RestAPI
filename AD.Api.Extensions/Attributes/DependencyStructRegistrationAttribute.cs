namespace AD.Api.Attributes.Services
{
    /// <summary>
    /// An attribute decorated on structs that will automatically register the type with the AD API application's
    /// dependency injection container at application startup binding it to the specified service type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class DependencyStructRegistrationAttribute : DependencyRegistrationAttribute
    {
        /// <summary>
        /// Indicates if the service type is valid to bind to the struct (i.e. must be an interface).
        /// </summary>
        public bool IsValid => this.ServiceType.IsInterface;

        /// <inheritdoc/>
        [NotNull]
        [DisallowNull]
        public required override Type ServiceType
        {
            [return: NotNull]
            get => base.ServiceType!;
            init => base.ServiceType = value ?? throw new ArgumentNullException(nameof(this.ServiceType));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyStructRegistrationAttribute"/> attribute class indicating
        /// that the decorated struct type is the implementation type for the specified service type when creating the
        /// <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="serviceType"><inheritdoc cref="ServiceType" path="/summary"/></param>
        [SetsRequiredMembers]
        public DependencyStructRegistrationAttribute(Type serviceType)
        {
            ArgumentNullException.ThrowIfNull(serviceType);
            this.ServiceType = serviceType;
        }
    }
}

