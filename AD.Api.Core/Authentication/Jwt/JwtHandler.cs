using AD.Api.Components;
using AD.Api.Core.Extensions;
using AD.Api.Core.Security.Encryption;
using AD.Api.Enums;
using AD.Api.Startup.Exceptions;
using AD.Api.Strings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Versioning;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using BCryptNet = BCrypt.Net.BCrypt;

namespace AD.Api.Core.Authentication.Jwt
{
    internal sealed class JwtHandler : TokenHandler
    {
        private readonly JwtAuthorizationService _authorizations;
        //private readonly JwtCache _cache;
        private TimeProvider _clock;
        private readonly JwtSecurityTokenHandler _handler;
        private readonly TokenValidationParameters _parameters;
        private readonly IEnumStrings<AuthorizedRole> _roles;
        private readonly CustomJwtSettings _settings;
        private readonly SigningCredentials _signingCreds;

        public TimeProvider Clock
        {
            get => _clock;
            set => _clock = value ?? TimeProvider.System;
        }
        public override int MaximumTokenSizeInBytes
        {
            get => _handler.MaximumTokenSizeInBytes;
            set => _handler.MaximumTokenSizeInBytes = value;
        }

        public JwtHandler(CustomJwtSettings settings, JwtAuthorizationService authorizations, IEnumStrings<AuthorizedRole> roles)
        {
            //_cache = new()
            _clock = TimeProvider.System;
            _handler = new();
            _roles = roles;
            _settings = settings;
            _authorizations = authorizations;

            byte[] plainBytes = GetSigningKey(settings);
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

        internal OneOf<(BearerToken, AuthorizedUser), IActionResult> CreateToken(IJwtLogin loginRequest)
        {
            Span<byte> byteBuffer = stackalloc byte[Base64Extensions.GetByteLength(loginRequest.Key.Length)];
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

            return (this.GenerateToken(user), user);
        }

        private BearerToken GenerateToken(AuthorizedUser user)
        {
            DateTimeOffset expiration = this.Clock.GetUtcNow().Add(_settings.TokenLifetime);
            SecurityTokenDescriptor descriptor = new()
            {
                IssuedAt = this.Clock.GetUtcNow().DateTime,
                Expires = expiration.DateTime,
                SigningCredentials = _signingCreds,
                Subject = new ClaimsIdentity([
                    new(ClaimTypes.NameIdentifier, user.UserName),
                    new(ClaimTypes.Role, _roles[user.Roles]),
                    new(ClaimTypes.Name, user.UserDisplayName),
                    new("scopes", string.Join(", ", user.Scopes))
                ]),
            };

            JwtSecurityToken token = _handler.CreateJwtSecurityToken(descriptor);
            string jwToken = _handler.WriteToken(token);

            return new BearerToken
            {
                Expires = expiration,
                Roles = user.Roles,
                Scopes = [.. user.Scopes],
                Token = jwToken,
            };
        }
        private static byte[] GetSigningKey(CustomJwtSettings settings)
        {
            if (!OperatingSystem.IsWindows() || !settings.SigningKeyDpApiScope.HasValue)
            {
                var certSvc = new CertificateEncryptionService();
                return certSvc.Decrypt(settings.SigningKey);
            }

            var dpapiSvc = new WindowsDpapiEncryptionService();
            return dpapiSvc.Decrypt(settings.SigningKey, settings.SigningKeyDpApiScope.Value);
        }

        public override SecurityToken ReadToken(string token)
        {
            return _handler.ReadToken(token);
        }
        public override Task<TokenValidationResult> ValidateTokenAsync(SecurityToken token, TokenValidationParameters validationParameters)
        {
            return _handler.ValidateTokenAsync(token, _parameters);
        }
        public override async Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
        {
            if (!_handler.CanReadToken(token))
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    Exception = new SecurityTokenException("Token cannot be read."),
                };
            }

            try
            {
                return await _handler.ValidateTokenAsync(token, _parameters).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    Exception = e,
                };
            }
        }
    }
}
