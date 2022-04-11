namespace AD.Api.Services
{
    public static class InjectServicesExtensions
    {
        public static IServiceCollection AddADApiServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IConnectionService, ConnectionService>()
                .AddSingleton<ISerializationService, SerializationService>()
                .AddTransient<IQueryService, LdapQueryService>()
                .AddTransient<IEditService, LdapEditService>();
        }
    }
}
