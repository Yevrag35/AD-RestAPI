using AD.Api.Components;
using AD.Api.Core.Authentication.Jwt;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.DirectoryServices.Protocols;
using System.IdentityModel.Tokens.Jwt;

namespace AD.Api.Services.Jwt
{
    internal sealed class NoJwtService : TokenHandler, IJwtService
    {
        private readonly OneOf<BearerToken, IActionResult> _result;
        private readonly NoSecurityToken _noToken;

        public TimeProvider Clock { get; set; } = TimeProvider.System;
        public bool IsFunctional => false;
        public override int MaximumTokenSizeInBytes
        {
            get => 0;
            set { }
        }

        public NoJwtService()
        {
            _result = OneOf<BearerToken>.FromT1<IActionResult>(new JwtNotEnabledResult());
            _noToken = new();
        }

        public OneOf<BearerToken, IActionResult> CreateToken(IJwtLogin loginRequest)
        {
            return _result;
        }

        public override SecurityToken ReadToken(string token)
        {
            return _noToken;
        }
        
        public override Task<TokenValidationResult> ValidateTokenAsync(SecurityToken token, TokenValidationParameters validationParameters)
        {
            return Task.FromResult(new TokenValidationResult
            {
                IsValid = false,
                Exception = new SecurityTokenException(Messages.JWT_NotEnabled)
            });
        }
        public override Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
        {
            return Task.FromResult(new TokenValidationResult
            {
                IsValid = false,
                Exception = new SecurityTokenException(Messages.JWT_NotEnabled)
            });
        }

        private sealed class JwtNotEnabledResult : ApiExceptionResult
        {
            public override string Error { get; }
            public override ResultCode Result { get; }
            public override int ResultCode { get; }

            internal JwtNotEnabledResult()
            {
                this.Error = Messages.JWT_NotEnabled;
                this.Result = System.DirectoryServices.Protocols.ResultCode.AuthMethodNotSupported;
                this.ResultCode = (int)this.Result;
            }

            protected override int GetStatusCode()
            {
                return StatusCodes.Status501NotImplemented;
            }
        }
        private sealed class NoSecurityToken : SecurityToken
        {
            private readonly SecurityKey _key;
            public override string Id => string.Empty;
            public override string Issuer => string.Empty;
            public override SecurityKey SecurityKey => _key;
            public override SecurityKey SigningKey
            {
                get => _key;
                set { }
            }

            public override DateTime ValidFrom => DateTime.MinValue;
            public override DateTime ValidTo => DateTime.MinValue.AddSeconds(1);

            public NoSecurityToken()
            {
                _key = new EmptyKey();
            }

            private sealed class EmptyKey : SecurityKey
            {
                public override int KeySize => 0;
                public override bool CanComputeJwkThumbprint()
                {
                    return false;
                }
                public override bool IsSupportedAlgorithm(string algorithm)
                {
                    return false;
                }
                public override byte[] ComputeJwkThumbprint()
                {
                    return [];
                }
                public override string KeyId
                {
                    get => string.Empty;
                    set { }
                }
            }
        }
    }
}
