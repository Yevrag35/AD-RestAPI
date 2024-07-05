using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Passwords;

public sealed class PasswordResetRequest
{
    [Required]
    [MinLength(4, ErrorMessage = "The distinguished name must be at least 4 characters long.")]
    [JsonRequired]
    [JsonPropertyName("dn")]
    public required string DistinguishedName { get; init; }

    [Required]
    [Base64String]
    [JsonRequired]
    public required string NewPassword { get; init; }
}

