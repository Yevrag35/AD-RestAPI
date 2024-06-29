using AD.Api.Core.Authentication.Jwt;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Authentication
{
    public sealed class LoginBody : IJwtLogin
    {
        [Required]
        [Base64String(ErrorMessage = "Key is not a valid base64 string.")]
        public required string Key { get; init; }

        [Required]
        [MinLength(1, ErrorMessage = "Username cannot be null or empty.")]
        public required string UserName { get; init; }
    }
}
