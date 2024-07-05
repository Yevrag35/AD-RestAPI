using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AD.Api.Core.Security.Accounts;

/// <summary>
/// Represents an account name formatted as a Down-level Logon Name (e.g., Domain\UserName).
/// </summary>
public sealed class DownLevelLogonName : AccountName
{
    private string? _combined;

    /// <summary>
    /// Gets or sets the domain name.
    /// </summary>
    public required string Domain { get; init; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DownLevelLogonName"/> class.
    /// </summary>
    public DownLevelLogonName()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DownLevelLogonName"/> class with the specified domain and user name.
    /// </summary>
    /// <param name="domain">The domain name.</param>
    /// <param name="userName">The user name.</param>
    [SetsRequiredMembers]
    public DownLevelLogonName(string domain, string userName)
    {
        this.Domain = domain;
        this.UserName = userName;
    }

    /// <summary>
    /// Sets the domain and user name properties of the provided network credential.
    /// </summary>
    /// <param name="credential">The network credential whose properties are to be set.</param>
    public override void SetCredential(NetworkCredential credential)
    {
        credential.Domain = this.Domain;
        credential.UserName = this.UserName;
    }

    /// <summary>
    /// Returns the username in the Down-level Logon Name format.
    /// </summary>
    /// <returns>A <see cref="string"/> in the Down-level Logon Name format: <c>Domain\UserName</c>.</returns>
    public override string ToString()
    {
        return _combined ??= $"{this.Domain}\\{this.UserName}";
    }

    /// <summary>
    /// Tries to validate the Down-level Logon Name.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="badResult">The validation result if the validation fails.</param>
    /// <returns><see langword="true"/> if the name is valid; otherwise, <see langword="false"/>.</returns>
    protected override bool TryValidateName(ValidationContext context, [NotNullWhen(false)] out ValidationResult? badResult)
    {
        badResult = null;
        if (string.IsNullOrWhiteSpace(this.Domain) || string.IsNullOrWhiteSpace(this.UserName))
        {
            badResult = new ValidationResult("Down-level logon names require both a NetBIOS domain name and a SamAccountName.", new[] { nameof(this.Domain), nameof(this.UserName) });
            return false;
        }

        return true;
    }
}

