using AD.Api.Core.Ldap.Results;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Passwords;

public sealed class PasswordChangeRequestByDN : PasswordChangeRequestBase, IPasswordRequest
{
    [Required]
    [MinLength(4, ErrorMessage = "The distinguished name must be at least 4 characters long.")]
    [JsonRequired]
    [JsonPropertyName("dn")]
    public required string DistinguishedName { get; init; }

    string IPasswordRequest.GetDistinguishedName() => this.DistinguishedName;
    bool IPasswordRequest.IsResetting() => false;
    bool IPasswordRequest.TryGetContinuation([NotNullWhen(true)] out ConnectedResponse? continuation)
    {
        continuation = null;
        return false;
    }
}