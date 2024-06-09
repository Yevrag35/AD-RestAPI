using AD.Api.Collections.Enumerators;
using AD.Api.Startup.Services;

namespace AD.Api.Attributes.Services
{
    /// <summary>
    /// A base attribute for defining services that will be automatically added to the dependency injection container
    /// including the service type, implementation type, and lifetime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public abstract class AddToDepedencyInjectionAttribute : AutomaticDependencyInjectionAttribute
    {
        /// <summary>
        /// The <see cref="Type"/> implementing the service.
        /// </summary>
        /// <remarks>
        /// Will default to the decorated type if not set or <see langword="null"/>.
        /// </remarks>
        protected Type? Implementation { get; set; }
        /// <inheritdoc cref="ServiceLifetime"/>
        public required ServiceLifetime Lifetime { get; init; }
        /// <summary>
        /// The <see cref="Type"/> of the service.
        /// </summary>
        /// <remarks>
        /// Will default to the decorated type if not set or <see langword="null"/>.
        /// </remarks>
        protected Type? Service { get; set; }

        /// <exception cref="ArgumentException"/>
        /// <inheritdoc 
        ///     cref="TryCreateDescriptorFromAttribute(AddToDepedencyInjectionAttribute, Type, in IServiceTypeExclusions, out ServiceDescriptor)"
        ///     path="/exception"/>
        [DebuggerStepThrough]
        public static IEnumerable<ServiceDescriptor> CreateDescriptorsFromType(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces)] Type type,
            IServiceTypeExclusions exclusions)
        {
            foreach (var attribute in type.GetCustomAttributes<AddToDepedencyInjectionAttribute>(inherit: false))
            {
                if (TryCreateDescriptorFromAttribute(attribute, type, in exclusions, out ServiceDescriptor? descriptor))
                {
                    yield return descriptor;
                }
            }
        }

        /// <inheritdoc cref="Type.GetGenericTypeDefinition" path="/exception"/>
        /// <inheritdoc cref="ValidateImplementationType(Type, Type)" path="/exception"/>
        private static bool TryCreateDescriptorFromAttribute(
            AddToDepedencyInjectionAttribute attribute,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces)] Type type,
            in IServiceTypeExclusions exclusions,
            [NotNullWhen(true)] out ServiceDescriptor? descriptor)
        {
            attribute.Service ??= type;
            attribute.Implementation ??= type;

            if (exclusions.IsExcluded(attribute.Service))
            {
                descriptor = null;
                return false;
            }

            if (attribute.Service.IsGenericTypeDefinition && !attribute.Implementation.IsGenericTypeDefinition)
            {
                attribute.Implementation = attribute.Implementation.GetGenericTypeDefinition();
            }

            ValidateImplementationType(attribute.Service, attribute.Implementation);
            descriptor = new(attribute.Service, attribute.Implementation, attribute.Lifetime);
            return true;
        }

        /// <exception cref="MissingConstructorException"></exception>
        /// <exception cref="TypeNotAssignableException"></exception>
        [DebuggerStepThrough]
        protected static void ValidateImplementationType(
            Type serviceType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces)] Type implementationType)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            ConstructorInfo[] ctors = implementationType.GetConstructors(flags);
            if (ctors.Length <= 0)
            {
                throw new MissingConstructorException(implementationType, flags);
            }
            else if (!ReferenceEquals(serviceType, implementationType) && !ImplementsType(serviceType, implementationType))
            {
                throw new TypeNotAssignableException(serviceType, implementationType);
            }
        }

        [DebuggerStepThrough]
        private static bool AnyGenericInterfaceMatches(
            Type serviceType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type implementationType)
        {
            ArrayRefEnumerator<Type> enumerator = new(implementationType.GetInterfaces());
            bool flag = false;

            while (enumerator.MoveNext(in flag))
            {
                Type type = enumerator.Current;
                if (!type.IsGenericTypeDefinition && type.IsGenericType)
                {
                    type = type.GetGenericTypeDefinition();
                }

                flag = type.IsGenericTypeDefinition
                       &&
                       type.Equals(serviceType);
            }

            return flag;
        }

        [DebuggerStepThrough]
        private static bool ImplementsType(
            Type serviceType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type implementationType)
        {
            if (serviceType.IsAssignableFrom(implementationType))
            {
                return true;
            }
            else if (serviceType.IsGenericTypeDefinition)
            {
                if (!implementationType.IsGenericTypeDefinition)
                {
                    implementationType = implementationType.GetGenericTypeDefinition();
                }

                if (serviceType.IsInterface)
                {
                    return AnyGenericInterfaceMatches(serviceType, implementationType);
                }
                else
                {
                    return implementationType.IsSubclassOf(serviceType);
                }
            }

            return false;
        }
    }
}

