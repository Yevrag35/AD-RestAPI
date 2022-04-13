using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;

namespace AD.Api.Settings
{
    public interface ITextSettings
    {
        Encoding Encoding { get; }
    }

    public static class TextSettingsDependencyInjection
    {
        public static IServiceCollection AddTextSettingOptions(this IServiceCollection services, IConfiguration config)
        {
            var settings = config.GetSection("Settings").GetSection("Text").Get<TextSettings>();
            services.AddSingleton(settings);
            return services.AddSingleton<ITextSettings>(services => services.GetRequiredService<TextSettings>());
        }

        private class TextSettings : ITextSettings
        {
            public string? Encoding
            {
                get => this.DefaultEncoding.HeaderName;
                set => this.SetEncoder(value);
            }
            Encoding ITextSettings.Encoding => this.DefaultEncoding;
            public Encoding DefaultEncoding { get; private set; } = System.Text.Encoding.UTF8;

            private void SetEncoder(string? encoding)
            {
                if (string.IsNullOrWhiteSpace(encoding))
                    return;

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
