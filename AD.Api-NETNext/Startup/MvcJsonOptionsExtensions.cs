using AD.Api.Core.Ldap;
using AD.Api.Core.Serialization;
using AD.Api.Core.Serialization.Json;
using AD.Api.Core.Settings;
using AD.Api.Extensions.Collections;
using AD.Api.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api
{
    public static class MvcJsonOptionsExtensions
    {
        public static IMvcBuilder AddADApiJsonConfiguration(this IMvcBuilder builder, IHostApplicationBuilder appBuilder, PropertyConverter converter)
        {
            IConfiguration config = appBuilder.Configuration;
            bool isDevelopment = appBuilder.Environment.IsDevelopment();
            LdapEnumConverter enumConverter = ConfigureAndAddEnumConverter(appBuilder);
            SerializationSettings settings = GetSerializationSettings(builder.Services, appBuilder.Configuration);

            return builder.AddJsonOptions(options =>
            {
                options.AllowInputFormatterExceptionMessages = isDevelopment;
                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonSpanCamelCaseNamingPolicy.SpanPolicy;
                options.JsonSerializerOptions.WriteIndented = settings.WriteIndented;

                options.JsonSerializerOptions.Converters.Add(enumConverter);
                AddAdditionalJsonConverters(options.JsonSerializerOptions, converter);
            });
        }

        private static void AddAdditionalJsonConverters(JsonSerializerOptions options, PropertyConverter converter)
        {
            options.Converters.AddRange([
                new ResultEntryConverter(converter),
                new ResultEntryCollectionConverter(converter),
            ]);
        }
        private static LdapEnumConverter ConfigureAndAddEnumConverter(IHostApplicationBuilder appBuilder)
        {
            var enumConverter = LdapEnumConverter.Create(options =>
            {
                options.SetNamingPolicy(JsonSpanCamelCaseNamingPolicy.SpanPolicy)
                       .Exclude<GroupType>()
                       .Exclude<SamAccountType>()
                       .Exclude<UserAccountControl>();
            });

            _ = appBuilder.Services.AddSingleton<LdapEnumConverter>(enumConverter);
            return enumConverter;
        }
        
        private static SerializationSettings GetSerializationSettings(IServiceCollection services, IConfiguration configuration)
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
