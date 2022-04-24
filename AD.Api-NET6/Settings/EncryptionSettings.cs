namespace AD.Api.Settings
{
    public class EncryptionSettings
    {
        public string? SHA1Thumbprint { get; set; }
    }

    public static class EncryptionSettingsDependencyInjection
    {
        public static IServiceCollection AddEncryptionOptions(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            return services.Configure<EncryptionSettings>(config => configurationSection.Bind(config));
        }
    }
}
