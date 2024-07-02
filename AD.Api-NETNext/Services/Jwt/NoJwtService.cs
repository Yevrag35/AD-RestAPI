using AD.Api.Components;
using AD.Api.Core.Authentication.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Services.Jwt
{
    internal sealed class NoJwtService : IJwtService
    {
        private static readonly ObjectResult _result = new(new
        {
            Message = "JWT authentication is not supported in this configuration.",
        })
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity,
        };

        public OneOf<string, IActionResult> CreateToken(IJwtLogin loginRequest)
        {
            return _result;
        }
    }
}
