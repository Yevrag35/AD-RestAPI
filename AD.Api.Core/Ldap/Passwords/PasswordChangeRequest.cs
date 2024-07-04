using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Passwords;

public sealed class PasswordChangeRequest
{
    [Required]
    [JsonRequired]
    [JsonPropertyName("dn")]
    public required string DistinguishedName { get; init; }

    [Required]
    [Base64String]
    [JsonRequired]
    public required string OldPassword { get; init; }

    [Required]
    [Base64String]
    [JsonRequired]
    public required string NewPassword { get; init; }
}