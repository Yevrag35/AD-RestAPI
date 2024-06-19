using AD.Api.Core.Ldap;
using AD.Api.Core.Serialization;
using AD.Api.Core.Serialization.Json;
using AD.Api.Core.Settings;
using AD.Api.Extensions.Collections;
using AD.Api.Serialization.Json;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Startup
{
    public static class MvcJsonOptionsExtensions
    {
        public static IMvcBuilder AddADApiJsonConfiguration(this IMvcBuilder builder, IHostApplicationBuilder appBuilder, PropertyConverter converter)
        {
            IConfiguration config = appBuilder.Configuration;
            bool isDevelopment = appBuilder.Environment.IsDevelopment();
            SerializationSettings settings = GetSerializationSettingsFromConfig(builder.Services, config);
            LdapEnumConverter enumConverter = LdapEnumConverter.Create(options =>
            {
                options.SetNamingPolicy(JsonSpanCamelCaseNamingPolicy.SpanPolicy)
                       .Exclude<GroupType>()
                       .Exclude<SamAccountType>()
                       .Exclude<UserAccountControl>();
            });

            return builder.AddJsonOptions(options =>
            {
                options.AllowInputFormatterExceptionMessages = isDevelopment;
                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonSpanCamelCaseNamingPolicy.SpanPolicy;
                options.JsonSerializerOptions.WriteIndented = settings.WriteIndented;
                options.JsonSerializerOptions.Converters.Add(enumConverter);

                AddJsonConverters(options.JsonSerializerOptions, converter);
            });
        }

        private static void AddJsonConverters(JsonSerializerOptions options, PropertyConverter converter)
        {
            options.Converters.AddRange([
                new ResultEntryConverter(converter),
                new ResultEntryCollectionConverter(converter),
            ]);
        }
        private static SerializationSettings GetSerializationSettingsFromConfig(IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection section = configuration
                .GetRequiredSection("Settings")
                .GetSection("Serialization");

            SerializationSettings? settings = section.Get<SerializationSettings>(x => x.ErrorOnUnknownConfiguration = false)
                ?? new SerializationSettings
                {
                    GuidAttributes = ["ms-DS-ConsistencyGuid", "objectGUID"],
                    WriteIndented = true,
                };

            services.AddSingleton(settings);
            return settings;
        }
    }
}
