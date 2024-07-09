using AD.Api.Core.Ldap.Requests;
using AD.Api.Core.Ldap.Results;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Passwords;

public sealed class PasswordChangeRequestByDN : PasswordChangeRequestBase, IPasswordRequest, IScopedRequest
{
    [Required]
    [MinLength(4, ErrorMessage = "The distinguished name must be at least 4 characters long.")]
    [JsonRequired]
    [JsonPropertyName("dn")]
    public required DistinguishedName DistinguishedName { get; init; }

    DistinguishedName IPasswordRequest.GetDistinguishedName()
    {
        return this.DistinguishedName;
    }
    DistinguishedName IScopedRequest.GetScopedPath()
    {
        return this.DistinguishedName;
    }
    string IScopedRequest.GetScopedPathMemberName()
    {
        return nameof(this.DistinguishedName);
    }
    bool IPasswordRequest.IsResetting()
    {
        return false;
    }
    bool IPasswordRequest.TryGetContinuation([NotNullWhen(true)] out ConnectedResponse? continuation)
    {
        continuation = null;
        return false;
    }
}