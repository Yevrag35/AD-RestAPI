using AD.Api.Core.Ldap.Results;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Passwords;

public sealed class PasswordChangeRequest : IPasswordRequest
{
	private ConnectedResponse? _response;

	[Required]
	[Base64String]
	[JsonRequired]
	public required string NewPassword { get; init; }
	[Required]
	[Base64String]
	[JsonRequired]
	public required string OldPassword { get; init; }

	bool IPasswordRequest.IsResetting() => false;
	public DistinguishedName GetDistinguishedName()
	{
		return _response?.FoundObject ?? DistinguishedName.Empty;
	}
	public void SetContinuation(ConnectedResponse response)
	{
		_response = response;
	}
	public bool TryGetContinuation([NotNullWhen(true)] out ConnectedResponse? continuation)
	{
		continuation = _response;
		return continuation is not null;
	}
}