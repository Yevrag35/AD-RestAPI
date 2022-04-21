using AD.Api.Ldap.Attributes;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Reflection;

namespace AD.Api.Services
{
    public static class InjectServicesExtensions
    {
        public static IServiceCollection AddADApiServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IConnectionService, ConnectionService>()
                .AddSingleton<IIdentityService, IdentityService>()
                .AddSingleton<IPasswordService, PasswordService>()
                .AddSingleton<IResultService, ResultService>()
                .AddSingleton<ISchemaService, SchemaService>()
                .AddSingleton<ISerializationService, SerializationService>()
                .AddScoped<ICreateService, LdapCreateService>()
                .AddScoped<IDeleteService, DeleteService>()
                .AddScoped<IEditService, LdapEditService>()
                .AddScoped<IMoveService, MoveService>()
                .AddScoped<IQueryService, LdapQueryService>()
                .AddScoped<IRenameService, RenameService>();
        }

        public static IServiceCollection AddLdapEnumTypes(this IServiceCollection services, Assembly[] assemblies)
        {
            ILdapEnumDictionary dict = EnumReader.GetLdapEnums(assemblies);
            return services
                .AddSingleton(dict);
        }

        public static IServiceCollection AddSwaggerWithOptions(this IServiceCollection services, IConfigurationSection openApiConfig,
            bool includeXmlComments = true)
        {
            var model = openApiConfig.Get<OpenApiInfo>();
            return services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", model);

                if (includeXmlComments)
                {
                    foreach (string xmlFile in GetXmlCommentFiles(AppContext.BaseDirectory))
                    {
                        options.IncludeXmlComments(xmlFile, true);
                    }
                }
            })
            .AddSwaggerGenNewtonsoftSupport();
        }

        private static IEnumerable<string> GetXmlCommentFiles(string baseDirectory)
        {
            return Directory.EnumerateFiles(baseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
        }
    }
}
