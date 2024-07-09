using AD.Api.Core.Ldap.Results;
using Microsoft.AspNetCore.Http;

namespace AD.Api.Core.Ldap.Passwords;

public interface IPasswordRequest
{
    string NewPassword { get; }
    string? OldPassword { get; }

    DistinguishedName GetDistinguishedName();
    [MemberNotNullWhen(false, nameof(OldPassword))]
    bool IsResetting();
    bool TryGetContinuation([NotNullWhen(true)] out ConnectedResponse? continuation);
}
