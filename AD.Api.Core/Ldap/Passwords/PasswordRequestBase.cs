using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap.Passwords;

public abstract class PasswordRequestBase
{
	[Required]
	[Base64String]
	[JsonRequired]
	public required string NewPassword { get; init; }
}

public abstract class PasswordChangeRequestBase : PasswordRequestBase
{
	[Required]
	[Base64String]
	[JsonRequired]
	public required string OldPassword { get; init; }
}
