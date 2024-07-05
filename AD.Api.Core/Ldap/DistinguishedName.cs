using AD.Api.Strings.Extensions;
using System.Buffers;

namespace AD.Api.Core.Ldap;

/// <summary>
/// Represents a distinguished name, with separate components for the common name and parent path.
/// </summary>
public sealed partial class DistinguishedName : IEquatable<DistinguishedName>
{
    /// <summary>
    /// Prefix for domain component in a distinguished name.
    /// </summary>
    public const string DomainComponentPrefix = "DC=";

    /// <summary>
    /// Prefix for common name in a distinguished name.
    /// </summary>
    public const string CommonNamePrefix = "CN=";

    /// <summary>
    /// Prefix for organizational unit in a distinguished name.
    /// </summary>
    public const string OrganizationalUnitPrefix = "OU=";

    private const int MAX_LENGTH = 400;

    private string? _fullValue;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _commonName;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _parentPath;

    //private static readonly char[] _dnChars = ;

    /// <summary>
    /// Characters that need to be escaped in a distinguished name.
    /// </summary>
    public static readonly SearchValues<char> EscapedChars = SearchValues.Create([',', '\\', '=', '+', '>', '<', ';', '"']);

    [MemberNotNullWhen(true, nameof(_fullValue))]
    private bool IsConstructed { get; set; }

    /// <summary>
    /// Gets or sets the common name component of the distinguished name.
    /// </summary>
    public string CommonName
    {
        get => _commonName;
        set
        {
            SetFieldValue(value, ref _commonName);
            this.ResetValue();
        }
    }

    /// <summary>
    /// Gets or sets the parent path component of the distinguished name.
    /// </summary>
    public string Path
    {
        get => _parentPath;
        set
        {
            SetFieldValue(value, ref _parentPath);
            this.ResetValue();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinguishedName"/> class with empty common name and parent path.
    /// </summary>
    public DistinguishedName()
    {
        _commonName = string.Empty;
        _parentPath = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinguishedName"/> class with the specified common name and parent path.
    /// </summary>
    /// <param name="commonName">The common name component.</param>
    /// <param name="parentPath">The parent path component.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="commonName"/> is null or whitespace.</exception>
    public DistinguishedName(string commonName, string? parentPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commonName);
        SetFieldValue(commonName, ref _commonName);
        SetFieldValue(parentPath, ref _parentPath);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinguishedName"/> class with the specified path.
    /// </summary>
    /// <param name="path">The full distinguished name path.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or whitespace.</exception>
    public DistinguishedName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        SetFieldValue(_commonName, ref _commonName);
        _parentPath = string.Empty;
    }

    private DistinguishedName(ReadOnlySpan<char> fullDn, ReadOnlySpan<char> cnSpan, ReadOnlySpan<char> parentSpan)
    {
        _fullValue = fullDn.ToString();
        _commonName = cnSpan.ToString();
        _parentPath = parentSpan.ToString();
        this.IsConstructed = true;
    }

    private string Construct()
    {
        scoped ReadOnlySpan<char> name = _commonName;
        if (name.IsWhiteSpace())
        {
            return _parentPath;
        }

        int extras = !string.IsNullOrWhiteSpace(_parentPath) ? 1 : 0;
        bool needsComma = extras == 1;
        bool needsPrefix = !name.StartsWith(CommonNamePrefix, StringComparison.OrdinalIgnoreCase);
        if (needsPrefix)
        {
            extras += CommonNamePrefix.Length;
        }

        int length = _commonName.Length + _parentPath.Length + extras;

        Constructing state = new(_commonName, _parentPath, needsComma, needsPrefix);
        return string.Create(length, state, ConstructingFullValue);
    }

    /// <summary>
    /// Determines whether the specified <see cref="DistinguishedName"/> is equal to the current <see cref="DistinguishedName"/>.
    /// </summary>
    /// <param name="other">The <see cref="DistinguishedName"/> to compare with the current <see cref="DistinguishedName"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="DistinguishedName"/> is equal to the current <see cref="DistinguishedName"/>; otherwise, <see langword="false"/>.</returns>
    public bool Equals(DistinguishedName? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        else if (other is null)
        {
            return false;
        }
        else
        {
            return this.ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="DistinguishedName"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="DistinguishedName"/>.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current <see cref="DistinguishedName"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is DistinguishedName dn)
        {
            return this.Equals(dn);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Serves as a hash function for the <see cref="DistinguishedName"/> type.
    /// </summary>
    /// <returns>A hash code for the current <see cref="DistinguishedName"/>.</returns>
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(this.ToString());
    }

    public int GetNumberOfRelativeNames()
    {
        ReadOnlySpan<char> dn = this.ToString();
        return CountNumberOfRelativeNames(dn);
    }

    private void ResetValue()
    {
        _fullValue = null;
        this.IsConstructed = false;
    }

    /// <summary>
    /// Returns the string representation of the full distinguished name.
    /// </summary>
    /// <returns>The string representation of the full distinguished name.</returns>
    public override string ToString()
    {
        if (!this.IsConstructed)
        {
            _fullValue = this.Construct();
            this.IsConstructed = true;
        }

        return _fullValue;
    }

    /// <summary>
    /// Parses a fully-formed distinguished name string into a <see cref="DistinguishedName"/> object.
    /// </summary>
    /// <param name="distinguishedName">The distinguished name string to parse.</param>
    /// <returns>A <see cref="DistinguishedName"/> object.</returns>
    public static DistinguishedName Parse(ReadOnlySpan<char> distinguishedName)
    {
        if (distinguishedName.IsWhiteSpace())
        {
            return new DistinguishedName();
        }

        int index = 0;
        while (index < distinguishedName.Length)
        {
            int commaIndex = distinguishedName.Slice(index).IndexOf(',');
            if (commaIndex < 0)
            {
                // No commas found at all, return the full DN as common name.
                return new DistinguishedName(distinguishedName.ToString());
            }

            // Adjust index relative to the original span.
            index += commaIndex;

            // Check if the comma is escaped.
            if (!distinguishedName.IsEscapedAt(in index))
            {
                // Found an unescaped comma.
                break;
            }

            index++;
        }

        if (index < 0 || index >= distinguishedName.Length - 3)
        {
            // No valid comma found or comma is at an invalid position.
            Debug.Fail("What is this?");
            return new DistinguishedName(distinguishedName.ToString());
        }

        ReadOnlySpan<char> commonName = distinguishedName.Slice(0, index);
        ReadOnlySpan<char> parentPath = distinguishedName.Slice(index + 1);
        return new DistinguishedName(distinguishedName, commonName, parentPath);
    }
}

