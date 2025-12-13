using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace AD.Api.Core.Authentication.Jwt;

public sealed class CustomJwtSettings
{
	public TimeSpan ExpirationSkew { get; init; } = TimeSpan.FromSeconds(30);
	public TimeSpan RenewalThreshold { get; init; } = TimeSpan.FromMinutes(30);
	public required string SigningKey { get; init; }
	[SupportedOSPlatform("WINDOWS")]
	public DataProtectionScope? SigningKeyDpApiScope { get; init; }
	public required TimeSpan TokenLifetime { get; init; }
	public required string Type { get; init; }

	public required JsonRoleBasedAccessControl RBAC { get; init; }
}
