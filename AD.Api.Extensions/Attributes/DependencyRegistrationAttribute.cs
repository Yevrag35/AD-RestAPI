namespace AD.Api.Attributes.Services
{
    /// <summary>
    /// An attribute decorated on classes that will automatically register the class with the AD API application's
    /// dependency injection container at application startup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DependencyRegistrationAttribute : AddToDepedencyInjectionAttribute
    {
        /// <summary>
        /// The service type that the decorated type will be registered as in the <see cref="ServiceCollection"/>.
        /// </summary>
        [MaybeNull]
        public virtual Type ServiceType
        {
            [return: MaybeNull]
            get => base.Service!;
            init => base.Service = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyRegistrationAttribute"/> attribute class indicating
        /// that the decorated class type is the service and implementation type when creating the
        /// <see cref="ServiceDescriptor"/>.
        /// </summary>
        public DependencyRegistrationAttribute()
        {
            this.ServiceType = null!;
            this.Lifetime = ServiceLifetime.Singleton;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyRegistrationAttribute"/> attribute class indicating
        /// that the decorated class type is the implementation type and the specified type is the service type when 
        /// creating the <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="serviceType">
        /// <inheritdoc cref="ServiceDescriptor(Type, Type, ServiceLifetime)" path="/param[1]"/>
        /// </param>
        public DependencyRegistrationAttribute(Type serviceType)
        {
            this.ServiceType = serviceType;
            this.Lifetime = ServiceLifetime.Singleton;
        }
    }
}

