using System.Security.Cryptography;

namespace AD.Api.Settings
{
    public interface IEncryptionSettings
    {
        RSAEncryptionPadding Padding { get; }
        string? SHA1Thumbprint { get; }
    }

    public class EncryptionSettings : IEncryptionSettings
    {
        public string? Padding { get; set; }
        RSAEncryptionPadding IEncryptionSettings.Padding => GetPaddingFromString(this.Padding);
        public string? SHA1Thumbprint { get; set; }

        private static RSAEncryptionPadding GetPaddingFromString(string? paddingStr)
        {
            switch (paddingStr?.ToUpper())
            {
                case "SHA1":
                    return RSAEncryptionPadding.OaepSHA1;

                case "SHA256":
                    goto default;

                case "SHA384":
                    return RSAEncryptionPadding.OaepSHA384;

                case "SHA512":
                    return RSAEncryptionPadding.OaepSHA512;

                case "PKCS1":
                    return RSAEncryptionPadding.Pkcs1;

                default:
                    return RSAEncryptionPadding.OaepSHA256;
            }
        }
    }

    public static class EncryptionSettingsDependencyInjection
    {
        public static IServiceCollection AddEncryptionOptions(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            IEncryptionSettings settings = configurationSection.Get<EncryptionSettings>();

            return services.AddSingleton(settings);
        }
    }
}
