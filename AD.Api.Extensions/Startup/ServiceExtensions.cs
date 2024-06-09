using AD.Api.Assemblies;
using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Collections.Enumerators;
using AD.Api.Exceptions.Startup;
using AD.Api.Reflection;
using AD.Api.Startup.Services.Internal;
using AD.Api.Startup.Services;

namespace AD.Api.Startup
{
    /// <summary>
    /// An extension class for adding services to a given <see cref="IServiceCollection"/> through the use
    /// of <see cref="AutomaticDependencyInjectionAttribute"/>-derived attributes.
    /// </summary>
    public static partial class ServiceExtensions
    {
        /// <summary>
        /// Adds services from types that are decorated with specific attributes to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        ///     Scans the <paramref name="assemblies"/> for types that are decorated with
        ///     <see cref="AutomaticDependencyInjectionAttribute"/> derivitives and adds them to the 
        ///     <paramref name="services"/> collection.  If a type is decorated with 
        ///     <see cref="DynamicDependencyRegistrationAttribute"/> then the method decorated with the 
        ///     <see cref="DependencyRegistrationMethodAttribute"/> will be invoked.
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns>
        ///     The same instance of the <see cref="IServiceCollection"/> for chaining.
        /// </returns>
        /// <inheritdoc cref="AddResolvedServicesFromAssembly(IServiceCollection, Assembly, IConfiguration, in IServiceTypeExclusions)" 
        ///     path="/exception"/>
        public static IServiceCollection AddResolvedServicesFromAssemblies(this IServiceCollection services, IConfiguration configuration, IEnumerable<Assembly> assemblies, Action<IAddServiceTypeExclusions>? configureExclusions = null)
        {
            IServiceTypeExclusions exclusions = ServiceTypeExclusions.ConfigureFromAction(configureExclusions);
            ServiceResolutionContext context = new(services, configuration, in exclusions);

            foreach (Assembly assembly in assemblies.Where(IsServicableAssembly))
            {
                AddResolvedServicesFromAssembly(assembly, in context);
            }

            return services;
        }

        /// <exception cref="DuplicatedServiceException"/>
        /// <exception cref="AdApiStartupException"></exception>
        private static void AddResolvedServicesFromAssembly(Assembly assembly, in ServiceResolutionContext context)
        {
            foreach (Type type in GetResolvableTypes(assembly, context.Exclusions))
            {
                if (!type.IsInterface && type.IsDefined(typeof(DynamicDependencyRegistrationAttribute), inherit: false))
                {
                    AddFromRegistration(in context, type);
                }
                else if (type.IsDefined(typeof(AddToDepedencyInjectionAttribute), inherit: false))
                {
                    try
                    {
                        foreach (var descriptor in AddToDepedencyInjectionAttribute.CreateDescriptorsFromType(type, context.Exclusions))
                        {
                            AddService(context.Services, descriptor);
                        }
                    }
                    catch (DuplicatedServiceException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw new AdApiStartupException(typeof(ServiceExtensions), $"Failed to create descriptor for type \"{type.GetName()}\".", e);
                    }
                }
            }
        }

        /// <exception cref="AdApiStartupException"></exception>
        private static void AddFromRegistration(
            in ServiceResolutionContext context,
            [DynamicallyAccessedMembers(
                DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] Type type)
        {
            MethodInfo? method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(x => x.IsDefined(typeof(DependencyRegistrationMethodAttribute), inherit: false))
                .OrderBy(x => x.Name)
                .FirstOrDefault();

            if (method is null)
            {
                Debug.Fail("No registration method found.");
                return;
            }

            try
            {
                Span<bool> twoBools = [false, false];
                BoolCounter counter = new(twoBools);
                CheckParameters(type, method, ref counter);

                context.InvokeStaticMethod(method, ref twoBools[^1]);
            }
            catch (Exception e)
            {
                throw new AdApiStartupException(typeof(ServiceExtensions), $"Failed to invoke registration method \"{method.Name}\".", e);
            }
        }

        /// <exception cref="InvalidOperationException"></exception>
        private static void CheckParameters(Type type, MethodInfo method, ref BoolCounter flags)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length <= 0)
            {
                throw new InvalidOperationException("Registration method must have at least the IServiceCollection parameter type");
            }
            else if (parameters.Length > 2)
            {
                throw new InvalidOperationException("Registration method must have at most two parameters");
            }

            ArrayRefEnumerator<ParameterInfo> enumerator = new(parameters);
            bool tripped = false;

            while (enumerator.MoveNext(in tripped))
            {
                ParameterInfo parameter = enumerator.Current;
                switch (parameter.Position)
                {
                    case 0:
                        flags.MarkFlag(0, typeof(IServiceCollection).Equals(parameter.ParameterType));
                        break;

                    case 1:
                        flags.MarkFlag(1, typeof(IConfiguration).Equals(parameter.ParameterType));
                        break;

                    default:
                        break;
                }

                tripped = flags.Count > 0;
            }

            if (0 == flags.Count)
            {
                throw new AdApiStartupException(type, Errors.Exception_InvalidMethodParameters);
            }
        }
        private static IEnumerable<Type> GetResolvableTypes(Assembly assembly, IServiceTypeExclusions exclusions)
        {
            return assembly.GetTypes()
                .Where(x => IsProperType(x)
                            &&
                            x.IsDefined(typeof(AutomaticDependencyInjectionAttribute), inherit: false)
                            &&
                            !exclusions.IsExcluded(x));
        }

        private static bool IsServicableAssembly(Assembly assembly)
        {
            return !assembly.IsDynamic
                && assembly.IsDefined(typeof(DependencyAssemblyAttribute), inherit: false);
        }
        private static bool IsProperType(Type type)
        {
            if (type.IsClass)
            {
                // Must not be static class.
                return !(type.IsAbstract && type.IsSealed);
            }
            else
            {
                // Must be interface or ValueType (struct).
                return type.IsValueType || type.IsInterface;
            }
        }

        #region ADD SERVICE

        /// <exception cref="DuplicatedServiceException"/>
        /// <exception cref="InvalidOperationException">
        ///     <paramref name="services"/> is read-only.
        /// </exception>
#if DEBUG
        private static void AddService(IServiceCollection services, ServiceDescriptor descriptor)
        {
            if (!_addedViaAttributes.Add(descriptor))
            {
                string msg = $"Duplicate service descriptor -> Service: {descriptor.ServiceType.GetName()}; Implementation: {descriptor.ImplementationType.GetName()}";
                Debug.Fail(msg);

                throw new DuplicatedServiceException(
                    serviceType: descriptor.ServiceType,
                    diType: typeof(ServiceExtensions));
            }

            services.Add(descriptor);
        }

        static readonly HashSet<ServiceDescriptor> _addedViaAttributes = new(100, new ServiceDescriptorComparer());
        private sealed class ServiceDescriptorComparer : IEqualityComparer<ServiceDescriptor>
        {
            public bool Equals(ServiceDescriptor? x, ServiceDescriptor? y)
            {
                return ReferenceEquals(x, y)
                       ||
                       (x is not null && y is not null
                        &&
                        x.ServiceType == y.ServiceType
                        &&
                        x.ImplementationType == y.ImplementationType);
            }

            public int GetHashCode([DisallowNull] ServiceDescriptor obj)
            {
                if (obj is null)
                {
                    return 0;
                }

                return HashCode.Combine(obj.ServiceType, obj.ImplementationType);
            }
        }
#else
        private static void AddService(IServiceCollection services, ServiceDescriptor descriptor)
        {
            services.Add(descriptor);
        }
#endif

        #endregion
    }
}

