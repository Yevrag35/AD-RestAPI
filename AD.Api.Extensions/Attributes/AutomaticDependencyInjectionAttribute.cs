using AD.Api.Attributes.Reflection;

namespace AD.Api.Attributes.Services
{
    /// <summary>
    /// The base class for all automatic dependency injection attributes.
    /// </summary>
    public abstract class AutomaticDependencyInjectionAttribute : AccessedViaReflectionAttribute
    {
        static readonly Type[] _serviceExtensions = [typeof(ServiceExtensions)];

        protected readonly record struct NamedArguments(ServiceLifetime Lifetime, Type? ImplementationType)
        {
            public static implicit operator NamedArguments((ServiceLifetime lifetime, Type? type) tuple)
            {
                return new(tuple.lifetime, tuple.type);
            }
        }

        protected AutomaticDependencyInjectionAttribute()
            : base(_serviceExtensions)
        {
        }
    }
}

