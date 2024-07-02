using AD.Api.Collections.Enumerators;
using AD.Api.Core.Authentication;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Enums;
using AD.Api.Middleware;
using AD.Api.Services.Enums;
using AD.Api.Services.Jwt;
using AD.Api.Startup.Exceptions;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace AD.Api.Startup
{
    internal static class AuthenticationStartupExtensions
    {
        internal static IServiceCollection AddApiAuthenticationAuthorization(this IServiceCollection services, ConfigurationManager configuration, out Action<WebApplication>? callback)
        {
            services.AddEnumStringDictionary<AuthorizedRole>(out var roles);
            IConfigurationSection authSection = configuration.GetRequiredSection("Authentication");
            callback = null;

            string? authType = authSection.GetValue("Type", string.Empty)?.ToUpperInvariant();
            switch (authType)
            {
                case "AD":
                case "NEGOTIATE":
                case "NTLM":
                    AddNegotiate(services, roles);
                    break;

                case "AAD":
                case "AZUREAD":
                case "ENTRA":
                case "ENTRAID":
                    AddEntraID(services, configuration);
                    break;

                case "CUSTOMJWT":
                case "JWT":
                    AddCustomJwt(services, authSection, roles);
                    break;

                case "KERBEROS":
                    callback = (app) => app.UseImpersonationMiddleware();
                    goto case "AD";

                default:
                    throw new AdApiStartupException(typeof(AuthenticationStartupExtensions),
                        "No authentication type was specified in the configuration file.");
            }

            return services;
        }
        private static void AddEntraID(IServiceCollection services, ConfigurationManager configuration)
        {
            IConfigurationSection entraIDSection = GetEntraIDSection(configuration);

            services.AddSingleton<IJwtService, NoJwtService>()
                    .AddAuthentication()
                    .AddMicrosoftIdentityWebApi(entraIDSection);
        }
        private static void AddCustomJwt(IServiceCollection services, IConfigurationSection authorizationSection, IEnumStrings<AuthorizedRole> roles)
        {
            services.AddJwtAuthentication(authorizationSection, roles);
        }
        private static void AddNegotiate(IServiceCollection services, IEnumStrings<AuthorizedRole> enumStrings)
        {
            services.AddSingleton<IAuthorizer, NegotiateAuthorizer>()
                    .AddSingleton<IJwtService, NoJwtService>()
                    .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                    .AddNegotiate(o => o.Validate());

            services.AddAuthorization(x =>
            {
                var policy = new AuthorizationPolicyBuilder(NegotiateDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                x.FallbackPolicy = policy;

                foreach (AuthorizedRole role in enumStrings.Values)
                {
                    x.AddPolicy(enumStrings[role], policy);
                }
            });
        }

        private static IConfigurationSection GetEntraIDSection(IConfiguration configuration)
        {
            string[]? array = null;
            Span<string> names = GetSectionNames(ref array);

            ArrayRefEnumerator<string> enumerator = new(names);
            IConfigurationSection section = null!;
            bool flag = false;

            while (enumerator.MoveNext(in flag))
            {
                section = configuration.GetSection(enumerator.Current);
                flag = section.Exists();
            }

            if (flag)
            {
                return section;
            }

            enumerator.Reset();
            configuration = configuration.GetRequiredSection("Authorization");

            while (enumerator.MoveNext(in flag))
            {
                section = configuration.GetSection(enumerator.Current);
                flag = section.Exists();
            }

            ArrayPool<string>.Shared.Return(array);
            return flag ? section : throw new AdApiStartupException(typeof(AuthenticationStartupExtensions),
                "No EntraID/AzureID settings sections were found in the configuration file.");
        }
        private static Span<string> GetSectionNames([NotNull] ref string[]? names)
        {
            names = ArrayPool<string>.Shared.Rent(4);
            names[0] = "AzureAD";
            names[1] = "EntraID";
            names[2] = "AAD";
            names[3] = "Entra";

            return names.AsSpan(0, 4);
        }
    }
}
