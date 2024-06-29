using System.Runtime.Versioning;

namespace AD.Api.Core.Authentication.Jwt
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class CustomJwtSettings
    {
        public required string SigningKey { get; init; }
        public TimeSpan ExpirationSkew { get; init; } = TimeSpan.FromSeconds(30);
        public required TimeSpan TokenLifetime { get; init; }
        public required string Type { get; init; }

        public required JsonRoleBasedAccessControl RBAC { get; init; }
    }
}
