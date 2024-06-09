using AD.Api.Enums;
using AD.Api.Enums.Internal;
using AD.Api.Ldap.Enums;

namespace AD.Api.Services.Enums
{
    public static class EnumStringDictionaryDependencyInjection
    {
        public static IServiceCollection AddEnumStringDictionary<T>(this IServiceCollection services) where T : unmanaged, Enum
        {
            return services.AddSingleton<IEnumStrings<T>>(provider =>
            {
                var dict = new ESDictionary<T>(isFrozen: true);
                return dict;
            });
        }
        public static IServiceCollection AddEnumStringDictionary<T>(this IServiceCollection services, bool freeze) where T : unmanaged, Enum
        {
            if (freeze)
            {
                return AddEnumStringDictionary<T>(services);
            }

            return services.AddSingleton<IEnumStrings<T>>(provider =>
            {
                var dict = new ESDictionary<T>(isFrozen: false);
                return dict;
            });
        }
        public static IServiceCollection AddEnumStringDictionary<T>(this IServiceCollection services, out IEnumStrings<T> constructed, bool freeze = true)
            where T : unmanaged, Enum
        {
            constructed = new ESDictionary<T>(isFrozen: freeze);
            return services.AddSingleton(constructed);
        }
        public static IServiceCollection AddEnumDictionaryGeneration(this IServiceCollection services, Action<IEnumStringRegistrator> registerExplicits)
        {
            ESDictionaryRegistrator registrator = new(services);
            registerExplicits(registrator);

            return services.AddSingleton(typeof(IEnumStrings<>), typeof(ESDictionary<>))
                           .AddSingleton(typeof(IEnumValues<,,>), typeof(EVDictionary<,,>));
        }
    }

    public interface IEnumStringRegistrator
    {
        IEnumStringRegistrator Register<T>() where T : unmanaged, Enum;
        IEnumStringRegistrator Register<T>(bool freeze) where T : unmanaged, Enum;
    }

    [DebuggerStepThrough]
    internal sealed class ESDictionaryRegistrator : IEnumStringRegistrator
    {
        private readonly IServiceCollection _services;

        internal ESDictionaryRegistrator(IServiceCollection services)
        {
            _services = services;
        }

        public IEnumStringRegistrator Register<T>() where T : unmanaged, Enum
        {
            EnumStringDictionaryDependencyInjection.AddEnumStringDictionary<T>(_services);
            return this;
        }
        public IEnumStringRegistrator Register<T>(bool freeze) where T : unmanaged, Enum
        {
            EnumStringDictionaryDependencyInjection.AddEnumStringDictionary<T>(_services, freeze);
            return this;
        }
    }
}

