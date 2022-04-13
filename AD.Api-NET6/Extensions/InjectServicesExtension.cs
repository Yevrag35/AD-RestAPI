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
    }
}
