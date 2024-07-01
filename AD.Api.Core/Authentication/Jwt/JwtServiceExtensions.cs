using AD.Api.Enums;
using AD.Api.Startup.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NLog;
using System.Collections.Frozen;
using System.Runtime.Versioning;
using System.Security.Claims;

namespace AD.Api.Core.Authentication.Jwt
{
    [SupportedOSPlatform("WINDOWS")]
    public static class JwtServiceExtensions
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
            IConfigurationSection authorizationSection, IEnumStrings<AuthorizedRole> roles)
        {
            CustomJwtSettings settings = authorizationSection.Get<CustomJwtSettings>()
                ?? throw new AdApiStartupException(typeof(JwtServiceExtensions), 
                    new NullReferenceException($"{nameof(CustomJwtSettings)} cannot be null."));

            return services.AddAuthorizationService()
                           .AddJsonFileAuthorization(settings)
                           .AddJwtAuthorization(roles);
        }

        private static IServiceCollection AddJsonFileAuthorization(this IServiceCollection services, CustomJwtSettings settings)
        {
            JsonRoleBasedAccessControl rbac = settings.RBAC;
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
                var e = new AdApiStartupException(typeof(JwtServiceExtensions), "Defined RBAC users has malformed/undefined authorization scopes set - Check the logs for the exact users/scopes.");
                _logger.Fatal(e);

                throw e;
            }

            return services.AddSingleton(scopes)
                           .AddSingleton(users)
                           .AddSingleton(settings);
        }
        private static IServiceCollection AddJwtAuthorization(this IServiceCollection 
            services, IEnumStrings<AuthorizedRole> roles)
        {
            return services.AddAuthorization(x =>
            {
                string[] schemes = [JwtBearerDefaults.AuthenticationScheme];
                x.FallbackPolicy = new AuthorizationPolicyBuilder(schemes[0])
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.Role)
                    .Build();

                x.AddPolicy(roles[AuthorizedRole.None], x.FallbackPolicy);
                x.AddPolicy(roles[AuthorizedRole.Reader], options =>
                {
                    options.RequireAuthenticatedUser()
                           .AddAuthenticationSchemes(schemes)
                           .RequireClaim(ClaimTypes.Role, [
                               roles[AuthorizedRole.Reader],
                               roles[AuthorizedRole.SuperAdmin],
                               roles[AuthorizedRole.ComputerAdmin],
                               roles[AuthorizedRole.GroupAdmin],
                               roles[AuthorizedRole.UserAdmin],
                           ]);
                });
                x.AddPolicy(roles[AuthorizedRole.SuperAdmin], options =>
                {
                    options.AddAuthenticationSchemes(schemes)
                           .RequireAuthenticatedUser()
                           .RequireClaim(ClaimTypes.Role, roles[AuthorizedRole.SuperAdmin]);
                });
                x.AddPolicy(roles[AuthorizedRole.UserAdmin], options =>
                {
                    options.AddAuthenticationSchemes(schemes)
                           .RequireAuthenticatedUser()
                           .RequireClaim(ClaimTypes.Role, [
                               roles[AuthorizedRole.UserAdmin],
                               roles[AuthorizedRole.SuperAdmin]
                           ]);
                });
                x.AddPolicy(roles[AuthorizedRole.GroupAdmin], options =>
                {
                    options.AddAuthenticationSchemes(schemes)
                           .RequireAuthenticatedUser()
                           .RequireClaim(ClaimTypes.Role, [
                               roles[AuthorizedRole.GroupAdmin],
                               roles[AuthorizedRole.SuperAdmin]
                           ]);
                });
                x.AddPolicy(roles[AuthorizedRole.ComputerAdmin], options =>
                {
                    options.AddAuthenticationSchemes(schemes)
                           .RequireAuthenticatedUser()
                           .RequireClaim(ClaimTypes.Role, [
                               roles[AuthorizedRole.ComputerAdmin],
                               roles[AuthorizedRole.SuperAdmin]
                           ]);
                });
            });
        }
    }
}

