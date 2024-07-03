using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Passwords;

public sealed class PasswordResetRequest
{
    [Required]
    [JsonPropertyName("dn")]
    public required string DistinguishedName { get; init; }

    [Base64String]
    [JsonRequired]
    public required string NewPassword { get; init; }
}

