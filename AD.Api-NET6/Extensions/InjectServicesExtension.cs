using AD.Api.Ldap.Attributes;
using System.Reflection;

namespace AD.Api.Services
{
    public static class InjectServicesExtensions
    {
        public static IServiceCollection AddADApiServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IConnectionService, ConnectionService>()
                .AddSingleton<ISchemaService, SchemaService>()
                .AddSingleton<ISerializationService, SerializationService>()
                .AddSingleton<IPasswordService, PasswordService>()
                .AddTransient<IQueryService, LdapQueryService>()
                .AddTransient<IEditService, LdapEditService>()
                .AddTransient<ICreateService, LdapCreateService>();
        }

        public static IServiceCollection AddLdapEnumTypes(this IServiceCollection services, Assembly[] assemblies)
        {
            ILdapEnumDictionary dict = EnumReader.GetLdapEnums(assemblies);
            return services
                .AddSingleton(dict);
        }
    }
}
