using AD.Api.Startup.Services;

namespace AD.Api.Startup
{
    public static partial class ServiceExtensions
    {
        private readonly struct ServiceResolutionContext
        {
            private readonly object[] _overload1;
            private readonly object[] _overload2;

            internal readonly IConfiguration Configuration;
            internal readonly IServiceTypeExclusions Exclusions;
            internal readonly IServiceCollection Services;

            internal ServiceResolutionContext(IServiceCollection services, IConfiguration configuration, in IServiceTypeExclusions exclusions)
            {
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

