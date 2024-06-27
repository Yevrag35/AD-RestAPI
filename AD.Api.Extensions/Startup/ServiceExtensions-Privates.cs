using AD.Api.Attributes.Services;
using AD.Api.Startup.Services;
using System.Runtime.Versioning;

namespace AD.Api.Startup
{
    public static partial class ServiceExtensions
    {
        private readonly struct ServiceResolutionContext
        {
            internal static readonly Type SupportedOSType = typeof(SupportedOSPlatformAttribute);

            private readonly object[] _overload1;
            private readonly object[] _overload2;

            internal readonly IConfiguration Configuration;
            internal readonly IServiceTypeExclusions Exclusions;
            internal readonly IServiceCollection Services;
            internal readonly Type MustHaveAttribute;

            internal ServiceResolutionContext(IServiceCollection services, IConfiguration configuration, in IServiceTypeExclusions exclusions)
            {
                MustHaveAttribute = typeof(AutomaticDependencyInjectionAttribute);
                Services = services;
                Configuration = configuration;
                Exclusions = exclusions;
                _overload1 = [services];
                _overload2 = [services, configuration];
            }

            /// <inheritdoc cref="MethodBase.Invoke(object?, object?[]?)" path="/exception"/>
            internal readonly void InvokeStaticMethod(MethodInfo method, ref readonly bool includeConfiguration)
            {
                object[] parameters = !includeConfiguration ? _overload1 : _overload2;
                _ = method.Invoke(null, parameters);
            }
        }
    }
}

