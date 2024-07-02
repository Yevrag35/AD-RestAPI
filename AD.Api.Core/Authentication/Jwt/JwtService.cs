using AD.Api.Components;
using AD.Api.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Versioning;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace AD.Api.Core.Authentication.Jwt
{
    public interface IJwtService
    {
        OneOf<string, IActionResult> CreateToken(IJwtLogin loginRequest);
    }

    [SupportedOSPlatform("WINDOWS")]
    internal sealed class JwtService : IJwtService
    {
        private readonly JwtAuthorizationService _authorizations;
        private readonly JwtSecurityTokenHandler _handler;
        private readonly TokenValidationParameters _parameters;
        private readonly IEnumStrings<AuthorizedRole> _roles;
        private readonly CustomJwtSettings _settings;
        private readonly SigningCredentials _signingCreds;

        public JwtService(CustomJwtSettings settings, JwtAuthorizationService authorizations, IEnumStrings<AuthorizedRole> roles)
        {
            _handler = new();
            _roles = roles;
            _settings = settings;
            _authorizations = authorizations;

            byte[] keyBytes = Convert.FromBase64String(settings.SigningKey);
            byte[] plainBytes = ProtectedData.Unprotect(keyBytes, null, DataProtectionScope.CurrentUser);
            SymmetricSecurityKey key = new(plainBytes);

            Array.Clear(plainBytes);

            _signingCreds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            _parameters = new()
            {
                ClockSkew = settings.ExpirationSkew,
                IssuerSigningKey = _signingCreds.Key,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        }

        public OneOf<string, IActionResult> CreateToken(IJwtLogin loginRequest)
        {
            Span<byte> byteBuffer = stackalloc byte[GetMaxBytes(loginRequest.Key.Length)];
            _ = Convert.TryFromBase64String(loginRequest.Key, byteBuffer, out int written);

            byteBuffer = byteBuffer.Slice(0, written);
            Span<char> chars = stackalloc char[Encoding.UTF8.GetCharCount(byteBuffer)];
            written = Encoding.UTF8.GetChars(byteBuffer, chars);

            if (!_authorizations.Users.TryGetValue(loginRequest.UserName, out var user))
            {
                return new UnauthorizedResult();
            }

            if (!BCryptNet.Verify(chars.Slice(0, written).ToString(), user.UserHash, hashType: user.HashType))
            {
                return new UnauthorizedResult();
            }

            return this.GenerateToken(user);
        }

        private string GenerateToken(AuthorizedUser user)
        {
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, user.UserName),
                        new Claim(ClaimTypes.Role, _roles[user.Roles]),
                        new Claim(ClaimTypes.Name, user.UserDisplayName),
                        new Claim("scopes", string.Join(", ", user.Scopes.Order())),
                    ]
                ),
                Expires = DateTime.UtcNow.Add(_settings.TokenLifetime),
                SigningCredentials = _signingCreds,
            };

            SecurityToken token = _handler.CreateToken(descriptor);
            return _handler.WriteToken(token);
        }

        private static int GetMaxBytes(int base64Length)
        {
            return (base64Length * 3) / 4;
        }
    }
}
