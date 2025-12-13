using AD.Api.Core.Ldap.Results;

namespace AD.Api.Core.Ldap.Passwords;

public sealed class PasswordResetRequest : PasswordRequestBase, IPasswordRequest
{
	private ConnectedResponse? _response;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string? IPasswordRequest.OldPassword => null;
	bool IPasswordRequest.IsResetting() => true;
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