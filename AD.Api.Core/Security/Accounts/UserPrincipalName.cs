using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AD.Api.Core.Security.Accounts;

/// <summary>
/// Represents an account name formatted as a User Principal Name (e.g., UserName@Domain).
/// </summary>
public sealed class UserPrincipalName : AccountName
{
    /// <summary>
    /// Gets or sets the user principal name.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPrincipalName"/> class.
    /// </summary>
    public UserPrincipalName()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPrincipalName"/> class with the specified user principal name.
    /// </summary>
    /// <param name="value">The user principal name.</param>
    [SetsRequiredMembers]
    public UserPrincipalName(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Sets the user principal name as the user name in the provided network credential, with an empty domain.
    /// </summary>
    /// <param name="credential">The network credential whose properties are to be set.</param>
    public override void SetCredential(NetworkCredential credential)
    {
        credential.Domain = string.Empty;
        credential.UserName = this.Value;
    }

    /// <summary>
    /// Returns the username in the User Principal Name format.
    /// </summary>
    /// <returns>A <see cref="string"/> in the User Principal Name format: <c>UserName@Domain</c>.</returns>
    public override string ToString()
    {
        return this.Value;
    }

    /// <summary>
    /// Tries to validate the User Principal Name.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="badResult">The validation result if the validation fails.</param>
    /// <returns><see langword="true"/> if the name is valid; otherwise, <see langword="false"/>.</returns>
    protected override bool TryValidateName(ValidationContext context, [NotNullWhen(false)] out ValidationResult? badResult)
    {
        badResult = null;
        if (string.IsNullOrWhiteSpace(this.Value) || this.Value.Length <= 3)
        {
            badResult = new ValidationResult("User Principal Names must be in the format: UserName@Domain.", new[] { nameof(this.Value) });
            return false;
        }

        ReadOnlySpan<char> chars = this.Value.AsSpan();
        int atIndex = chars.IndexOf('@');
        if (atIndex <= 0)
        {
            badResult = new ValidationResult("User Principal Names must be in the format: UserName@Domain.", new[] { nameof(this.Value) });
            return false;
        }

        chars = chars.Slice(atIndex);
        if (chars.Length <= 1)
        {
            badResult = new ValidationResult("User Principal Names must have a domain name.", new[] { nameof(this.Value) });
            return false;
        }

        return true;
    }
}
