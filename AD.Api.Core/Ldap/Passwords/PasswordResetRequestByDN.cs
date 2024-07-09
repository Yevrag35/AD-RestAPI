using AD.Api.Core.Ldap.Results;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Passwords;

public sealed class PasswordResetRequestByDN : PasswordRequestBase, IPasswordRequest
{
    [Required]
    [MinLength(4, ErrorMessage = "The distinguished name must be at least 4 characters long.")]
    [JsonRequired]
    [JsonPropertyName("dn")]
    public required DistinguishedName DistinguishedName { get; init; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string? IPasswordRequest.OldPassword => null;
    DistinguishedName IPasswordRequest.GetDistinguishedName() => this.DistinguishedName;
    bool IPasswordRequest.IsResetting() => true;
    bool IPasswordRequest.TryGetContinuation([NotNullWhen(true)] out ConnectedResponse? continuation)
    {
        continuation = null;
        return false;
    }
}

