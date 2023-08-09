using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace AD.Api.Settings
{
    public interface ITextSettings
    {
        DateTimeZoneHandling DateTimeZoneHandling { get; }
        Encoding Encoding { get; }
        NamingStrategy LdapEditNamingStrategy { get; }
        NamingStrategy LdapPropertyNamingStrategy { get; }
        NamingStrategy StringEnumNamingStrategy { get; }
    }

    public static class TextSettingsDependencyInjection
    {
        public static IServiceCollection AddTextSettingOptions(this IServiceCollection services, IConfiguration config, out ITextSettings textSettings)
        {
            textSettings = config.GetSection("Settings").GetSection("Text").Get<TextSettings>();
            return services.AddSingleton(textSettings);
        }

        private class TextSettings : ITextSettings
        {
            private static readonly Lazy<DefaultNamingStrategy> _default = new();
            private static readonly Lazy<CamelCaseNamingStrategy> _camelCase = new();

            public string? DateTimeHandling { get; set; }
            DateTimeZoneHandling ITextSettings.DateTimeZoneHandling
            {
                get
                {
                    switch (this.DateTimeHandling?.ToLower())
                    {
                        case "serverlocal":
                        case "local":
                            return DateTimeZoneHandling.Local;

                        case "preserve":
                        case "roundtripkind":
                            return DateTimeZoneHandling.RoundtripKind;

                        case "utc":
                            return DateTimeZoneHandling.Utc;

                        default:
                            goto case "local";
                    }
                }
            }
            public string? Encoding
            {
                get => this.DefaultEncoding.HeaderName;
                set => this.SetEncoder(value);
            }
            Encoding ITextSettings.Encoding => this.DefaultEncoding;
            public Encoding DefaultEncoding { get; private set; } = System.Text.Encoding.UTF8;

            NamingStrategy ITextSettings.LdapEditNamingStrategy => _camelCase.Value;
            public string? LdapPropertyNamingStrategy { get; set; }
            NamingStrategy ITextSettings.LdapPropertyNamingStrategy
            {
                get => GetNamingStrategy(this.LdapPropertyNamingStrategy);
            }

            public string? StringEnumNamingStrategy { get; set; }
            NamingStrategy ITextSettings.StringEnumNamingStrategy
            {
                get => GetNamingStrategy(this.StringEnumNamingStrategy);
            }

            private static NamingStrategy GetNamingStrategy(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return _default.Value;
                }

                switch (value.ToLower())
                {
                    case "camel":
                    case "camelcase":
                        return _camelCase.Value;

                    case "kebab":
                        return new KebabCaseNamingStrategy();

                    case "snake":
                    case "nosteponsnek":
                    case "snakecase":
                        return new SnakeCaseNamingStrategy();

                    default:
                        return _default.Value;
                }
            }

            private void SetEncoder(string? encoding)
            {
                if (string.IsNullOrWhiteSpace(encoding))
                {
                    return;
                }

                System.Text.Encoding enc;
                try
                {
                    enc = System.Text.Encoding.GetEncoding(encoding);
                }
                catch
                {
                    return;
                }

                this.DefaultEncoding = enc;
            }
        }
    }
}
