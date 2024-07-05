using AD.Api.Statics;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace AD.Api.Core.Security.Accounts;

/// <summary>
/// Represents an account name and provides a method to set credentials.
/// </summary>
public interface IAccountName : IValidatableObject
{
    /// <summary>
    /// Sets the network credential properties such as UserName and Domain for the account.
    /// </summary>
    /// <param name="credential">The network credential to set.</param>
    void SetCredential(NetworkCredential credential);
}

/// <summary>
/// Abstract base class for account names, providing methods to parse account names and validate them.
/// </summary>
public abstract class AccountName : IAccountName, IValidatableObject
{
    /// <summary>
    /// Parses a username from a read-only span of characters into an <see cref="IAccountName"/> instance.
    /// </summary>
    /// <param name="userName">The username to parse.</param>
    /// <returns>An <see cref="IAccountName"/> instance representing the parsed username.</returns>
    public static IAccountName Parse(ReadOnlySpan<char> userName)
    {
        char slash = CharConstants.BACKSLASH;
        if (userName.Contains(slash) && slash != userName[^1])
        {
            int index = userName.IndexOf(slash);
            ReadOnlySpan<char> domain = userName.Slice(0, index);
            ReadOnlySpan<char> name = userName.Slice(index + 1);
            return new DownLevelLogonName(domain.ToString(), name.ToString());
        }
        else if (userName.Contains('@'))
        {
            return new UserPrincipalName(userName.ToString());
        }
        else
        {
            return Empty;
        }
    }

    /// <summary>
    /// Parses a username from a read-only span of encoded bytes into an <see cref="IAccountName"/> instance.
    /// </summary>
    /// <param name="encodedBytes">The encoded bytes representing the username.</param>
    /// <param name="encoding">The encoding used for the bytes.</param>
    /// <returns>An <see cref="IAccountName"/> instance representing the parsed username.</returns>
    public static IAccountName Parse(ReadOnlySpan<byte> encodedBytes, Encoding encoding)
    {
        int length = encoding.GetMaxCharCount(encodedBytes.Length);
        Span<char> chars = stackalloc char[length];

        int written = encoding.GetChars(encodedBytes, chars);

        return Parse(chars.Slice(0, written));
    }

    /// <summary>
    /// Allows implementations to set the properties of the provided network credential, such as UserName and Domain.
    /// </summary>
    /// <param name="credential">The network credential whose properties are to be set.</param>
    public abstract void SetCredential(NetworkCredential credential);

    /// <summary>
    /// Validates the account name.
    /// </summary>
    /// <param name="validationContext">The context in which the validation is performed.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!this.TryValidateName(validationContext, out ValidationResult? badResult))
        {
            yield return badResult;
        }
    }

    /// <summary>
    /// Tries to validate the account name.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="badResult">The validation result if the validation fails.</param>
    /// <returns><see langword="true"/> if the name is valid; otherwise, <see langword="false"/>.</returns>
    protected abstract bool TryValidateName(ValidationContext context, [NotNullWhen(false)] out ValidationResult? badResult);

    /// <summary>
    /// Represents an empty account name.
    /// </summary>
    public static readonly IAccountName Empty = new EmptyName();

    /// <summary>
    /// Struct representing an empty account name.
    /// </summary>
    private readonly struct EmptyName : IAccountName
    {
        /// <summary>
        /// This implementation should never be used to set credentials.
        /// </summary>
        /// <param name="credential">The network credential whose properties are to be set.</param>
        public void SetCredential(NetworkCredential credential)
        {
            Debug.Fail("EmptyName should never be used to set credentials.");
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return [];
        }
    }
}