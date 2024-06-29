using AD.Api.Core.Authentication;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Enums;
using AD.Api.Startup.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System.Collections.Frozen;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AD.Api.Settings
{
    public static class JwtAuthenticationExtensions
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void AddJwtAuthentication(this WebApplicationBuilder builder, IEnumStrings<AuthorizedRole> enumStrings)
        {
            IConfigurationSection section = builder.Configuration.GetRequiredSection("Authorization");

            CustomJwtSettings settings = section.Get<CustomJwtSettings>()
                ?? throw new AdApiStartupException(typeof(JwtAuthenticationExtensions));

            builder.Services.AddSingleton(settings);

            AddJsonFileAuthorization(builder, settings.RBAC);

            byte[] base64Bytes = Convert.FromBase64String(settings.SigningKey);
            byte[] plainBytes = ProtectedData.Unprotect(base64Bytes, null, DataProtectionScope.CurrentUser);
            SymmetricSecurityKey key = new(plainBytes);

            Array.Clear(plainBytes);

            builder.Services.AddAuthentication()
                            .AddJwtBearer(x =>
                            {
                                x.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ClockSkew = settings.ExpirationSkew,
                                    IssuerSigningKey = key,
                                    ValidateIssuerSigningKey = true,
                                    ValidateIssuer = false,
                                    ValidateAudience = false,
                                };
                            });

            builder.Services.AddAuthorization(x =>
            {
                string[] schemes = [JwtBearerDefaults.AuthenticationScheme];
                x.FallbackPolicy = new AuthorizationPolicyBuilder(schemes[0])
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.Role)
                    .Build();

                x.AddPolicy(enumStrings[AuthorizedRole.None], x.FallbackPolicy);
                x.AddPolicy(enumStrings[AuthorizedRole.Reader], options =>
                {
                    options.RequireAuthenticatedUser()
                           .AddAuthenticationSchemes(schemes)
                           .RequireClaim(ClaimTypes.Role, [
                               enumStrings[AuthorizedRole.Reader],
                               enumStrings[AuthorizedRole.SuperAdmin],
                               enumStrings[AuthorizedRole.ComputerAdmin],
                               enumStrings[AuthorizedRole.GroupAdmin],
                               enumStrings[AuthorizedRole.UserAdmin],
                           ]);
                });
                x.AddPolicy(enumStrings[AuthorizedRole.SuperAdmin], options =>
                {
                    options.AddAuthenticationSchemes(schemes)
                           .RequireAuthenticatedUser()
                           .RequireClaim(ClaimTypes.Role, enumStrings[AuthorizedRole.SuperAdmin]);
                });
                x.AddPolicy(enumStrings[AuthorizedRole.UserAdmin], options =>
                {
                    options.AddAuthenticationSchemes(schemes)
                           .RequireAuthenticatedUser()
                           .RequireClaim(ClaimTypes.Role, [
                               enumStrings[AuthorizedRole.UserAdmin],
                               enumStrings[AuthorizedRole.SuperAdmin]
                           ]);
                });
                x.AddPolicy(enumStrings[AuthorizedRole.GroupAdmin], options =>
                {
                    options.AddAuthenticationSchemes(schemes)
                           .RequireAuthenticatedUser()
                           .RequireClaim(ClaimTypes.Role, [
                               enumStrings[AuthorizedRole.GroupAdmin],
                               enumStrings[AuthorizedRole.SuperAdmin]
                           ]);
                });
                x.AddPolicy(enumStrings[AuthorizedRole.ComputerAdmin], options =>
                {
                    options.AddAuthenticationSchemes(schemes)
                           .RequireAuthenticatedUser()
                           .RequireClaim(ClaimTypes.Role, [
                               enumStrings[AuthorizedRole.ComputerAdmin],
                               enumStrings[AuthorizedRole.SuperAdmin]
                           ]);
                });
            });
        }

        private static void AddJsonFileAuthorization(WebApplicationBuilder builder, JsonRoleBasedAccessControl rbac)
        {
            var scopes = rbac.Scopes.ToFrozenDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
            var users = rbac.Users.ToFrozenDictionary(x => x.UserName, StringComparer.OrdinalIgnoreCase);

            bool hasBadScopes = false;
            HashSet<string> set = new(5, StringComparer.OrdinalIgnoreCase);
            foreach (AuthorizedUser user in users.Values.Where(x => x.Scopes.Length > 0))
            {
                set.Clear();
                set.UnionWith(user.Scopes);
                set.ExceptWith(scopes.Keys);

                if (set.Count > 0)
                {
                    hasBadScopes = true;
                    _logger.Error("User {Name} has scopes that are not defined in the RBAC configuration: {Scopes}", user.UserName, string.Join(", ", set.Order()));
                }
            }

            if (hasBadScopes)
            {
                var e = new AdApiStartupException(typeof(JwtAuthenticationExtensions), "Defined RBAC users has malformed/undefined authorization scopes set - Check the logs for the exact users/scopes.");
                _logger.Fatal(e);

                throw e;
            }

            builder.Services.AddSingleton(scopes).AddSingleton(users);

            //return Task.CompletedTask;
        }
    }
}
