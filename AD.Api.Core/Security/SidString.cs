using AD.Api.Spans;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using AD.Api.Strings.Spans;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace AD.Api.Core.Security;

/// <summary>
/// Represents a security identifier (SID) string and provides methods for formatting and comparison.
/// </summary>
public sealed class SidString : IEquatable<SidString>, ISpanFormattable
{
    private static readonly char L_FORMAT = 'L';
    private const int SID_MAX_LENGTH = 189;
    public static ReadOnlySpan<char> LdapFormat => new(in L_FORMAT);
    public static ReadOnlySpan<char> SidFormat => default;

    /// <summary>
    /// The maximum number of characters in a SID string.
    /// </summary>
    /// <remarks>
    /// <c>S-</c> = 2 characters<br/>
    /// Revision: <c>1-</c> = 2 characters<br/>
    /// Identifier Authority = 20 characters (MAX)<br/>
    /// 15 Sub-authorities separated by <c>-</c> = 10 characters (MAX) each.
    /// </remarks>
    /// <value>
    /// <c>189</c>
    /// </value>
    public static readonly int MaxSidStringLength = SID_MAX_LENGTH;

    private readonly string _rawString;
    private string? _ldapString;

    /// <summary>
    /// Gets the string representation of the SID in Security Descriptor Definition Language (SDDL) format.
    /// </summary>
    public string Value => _rawString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SidString"/> class from a string in the Security Descriptor Definition Language (SDDL) format.
    /// </summary>
    /// <param name="sddlForm">
    /// The string representation of the SID in Security Descriptor Definition Language (SDDL) format.
    /// </param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="sddlForm"/> is null, empty, or does not start with 'S-'.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="sddlForm"/> is null.</exception>
    public SidString(string sddlForm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sddlForm);
        if (sddlForm.Length > SID_MAX_LENGTH)
        {
            throw new ArgumentException("The length of the SID string is too long.", nameof(sddlForm));
        }
        else if (!sddlForm.AsSpan().StartsWith("S-".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The SID string does not start with 'S-'.", nameof(sddlForm));
        }

        _rawString = sddlForm.ToUpperInvariant();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SidString"/> class from a binary form of a SID.
    /// </summary>
    /// <param name="binaryForm">The binary representation of the SID.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the length of <paramref name="binaryForm"/> is less than 8.</exception>
    public SidString(byte[] binaryForm)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(binaryForm.Length, 8, nameof(binaryForm));
        Span<char> span = stackalloc char[SID_MAX_LENGTH];
        int written = FormatSpan(span, binaryForm);
        _rawString = new string(span.Slice(0, written));
    }

    /// <summary>
    /// Indicates whether the current SID string is equal to another SID string.
    /// </summary>
    /// <param name="other">The SID string to compare to this instance.</param>
    /// <returns><see langword="true"/> if the current SID string is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals([NotNullWhen(true)] SidString? other)
    {
        if (RefEqualsOrNull(this, other, out bool result))
        {
            return result;
        }

        return StringComparer.OrdinalIgnoreCase.Equals(_rawString, other._rawString);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current SID string.
    /// </summary>
    /// <param name="obj">The object to compare with the current SID string.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current SID string; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (RefEqualsOrNull(this, obj, out bool result))
        {
            return result;
        }
        else if (obj is SidString other)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(_rawString, other._rawString);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a hash code for the current SID string.
    /// </summary>
    /// <returns>A hash code for the current SID string.</returns>
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(_rawString);
    }

    /// <summary>
    /// Formats the binary form of a SID into its string representation.
    /// </summary>
    /// <param name="destination">The destination span to write the formatted SID string.</param>
    /// <param name="sidBytes">The binary representation of the SID.</param>
    /// <returns>The number of characters written to the destination span.</returns>
    public static int FormatSpan(Span<char> destination, ReadOnlySpan<byte> sidBytes)
    {
        if (sidBytes.Length < 8)
        {
            return 0;
        }

        // Revision
        byte revision = sidBytes[0];

        // Sub-authority count
        byte subAuthorityCount = sidBytes[1];

        // Identifier authority
        ulong identifierAuthority = (ulong)sidBytes[2] << 40 | (ulong)sidBytes[3] << 32 |
                                    (ulong)sidBytes[4] << 24 | (ulong)sidBytes[5] << 16 |
                                    (ulong)sidBytes[6] << 8 | (ulong)sidBytes[7];

        // Span to store the resulting SID string
        int position = 0;
        Span<char> start = ['S', CharConstants.HYPHEN];
        start.CopyToSlice(destination, ref position);

        _ = revision.TryFormat(destination.Slice(position), out int written);
        position += written;
        destination[position++] = CharConstants.HYPHEN;

        _ = identifierAuthority.TryFormat(destination.Slice(position), out written);
        position += written;

        // Append sub-authorities
        for (int i = 0; i < subAuthorityCount; i++)
        {
            uint subAuthority = BitConverter.ToUInt32(sidBytes.Slice(8 + i * 4, 4));

            destination[position++] = CharConstants.HYPHEN;
            _ = subAuthority.TryFormat(destination.Slice(position), out written);
            position += written;
        }

        return position;
    }

    /// <summary>
    /// Checks if two objects are reference equals or if the other object is null.
    /// </summary>
    /// <param name="this">The current SID string instance.</param>
    /// <param name="other">The object to compare.</param>
    /// <param name="result">The result of the reference equality or null check.</param>
    /// <returns><see langword="true"/> if the objects are reference equals or if the other object is null; otherwise, <see langword="false"/>.</returns>
    private static bool RefEqualsOrNull(SidString @this, [NotNullWhen(false)] object? other, out bool result)
    {
        if (ReferenceEquals(@this, other))
        {
            result = true;
            return result;
        }
        else if (other is null)
        {
            result = false;
            return true;
        }
        else
        {
            result = false;
            return result;
        }
    }

    /// <summary>
    /// Gets the LDAP string representation of the SID.
    /// </summary>
    /// <returns>The LDAP string representation of the SID.</returns>
    [SupportedOSPlatform("WINDOWS")]
    public string ToLdapString()
    {
        return _ldapString ??= CreateLdapString(new SecurityIdentifier(_rawString));
    }

    /// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
    [SupportedOSPlatform("WINDOWS")]
    string IFormattable.ToString(string? format, IFormatProvider? provider)
    {
        return this.ToLdapString();
    }

    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
    [SupportedOSPlatform("WINDOWS")]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default)
    {
        if (!format.IsEmpty && format.Equals(LdapFormat, StringComparison.OrdinalIgnoreCase))
        {
            if (_ldapString is not null)
            {
                return _ldapString.TryCopyTo(destination, out charsWritten);
            }

            WriteLdapToSpan(ref destination, new SecurityIdentifier(_rawString), out charsWritten);
            return charsWritten > 0;
        }

        return _rawString.TryCopyTo(destination, out charsWritten);
    }

    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
    [SupportedOSPlatform("WINDOWS")]
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return this.TryFormat(destination, out charsWritten);
    }

    /// <summary>
    /// Creates an LDAP string representation of the specified <see cref="SecurityIdentifier"/>.
    /// </summary>
    /// <param name="sid">The <see cref="SecurityIdentifier"/> to create the LDAP string for.</param>
    /// <returns>The LDAP string representation of the specified <see cref="SecurityIdentifier"/>.</returns>
    [SupportedOSPlatform("WINDOWS")]
    private static string CreateLdapString(SecurityIdentifier sid)
    {
        SpanStringBuilder builder = new(sid.BinaryLength * 3);

        byte[] borrow = ArrayPool<byte>.Shared.Rent(sid.BinaryLength);
        sid.GetBinaryForm(borrow, 0);

        FormatByteArrayToSpan(borrow.AsSpan(0, sid.BinaryLength), ref builder);

        string result = builder.Build();
        ArrayPool<byte>.Shared.Return(borrow);

        return result;
    }

    /// <summary>
    /// Formats a byte array into a span using hexadecimal representation.
    /// </summary>
    /// <param name="byteArray">The byte array to format.</param>
    /// <param name="builder">The span builder to append the formatted byte array.</param>
    private static void FormatByteArrayToSpan(Span<byte> byteArray, ref SpanStringBuilder builder)
    {
        foreach (byte b in byteArray)
        {
            builder = builder.Append(3, b, (span, singleByte) =>
            {
                int bufferIndex = 0;
                span[bufferIndex++] = '\\';
                span[bufferIndex++] = GetHexValue(singleByte / 16);
                span[bufferIndex++] = GetHexValue(singleByte % 16);
            });
        }
    }

    /// <summary>
    /// Gets the hexadecimal character for the specified value.
    /// </summary>
    /// <param name="i">The value to convert to a hexadecimal character.</param>
    /// <returns>The hexadecimal character for the specified value.</returns>
    private static char GetHexValue(int i)
    {
        return i < 10
            ? (char)(i + '0')
            : (char)(i - 10 + 'A');
    }

    /// <summary>
    /// Writes the LDAP string representation of the specified <see cref="SecurityIdentifier"/> to the destination span.
    /// </summary>
    /// <param name="destination">The destination span to write the LDAP string to.</param>
    /// <param name="sid">The <see cref="SecurityIdentifier"/> to create the LDAP string for.</param>
    /// <param name="written">The number of characters written to the destination span.</param>
    [SupportedOSPlatform("WINDOWS")]
    private static void WriteLdapToSpan(ref Span<char> destination, SecurityIdentifier sid, out int written)
    {
        SpanStringBuilder builder = new(destination);

        byte[] borrow = ArrayPool<byte>.Shared.Rent(sid.BinaryLength);
        sid.GetBinaryForm(borrow, 0);

        FormatByteArrayToSpan(borrow.AsSpan(0, sid.BinaryLength), ref builder);
        destination = builder.AsSpan();

        ArrayPool<byte>.Shared.Return(borrow);
        written = builder.Length;
    }
}
