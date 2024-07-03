using AD.Api.Spans;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using AD.Api.Strings.Spans;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace AD.Api.Core.Security;

public sealed class SidString : IEquatable<SidString>, ISpanFormattable
{
    private static readonly char L_FORMAT = 'L';
    private const int SID_MAX_LENGTH = 189;
    public static ReadOnlySpan<char> LdapFormat => new(in L_FORMAT);
    public static ReadOnlySpan<char> SidFormat => default;

    /// <summary>
    /// 2+2+20+15(1+10)=189 maximum number of characters in a SID string.
    /// </summary>
    /// <remarks>
    /// <c>S-</c> = 2 characters<br/>
    /// Revision: <c>1-</c> = 2 characters<br/>
    /// Identifier Authority = 20 characters (MAX)<br/>
    /// 15 Sub-authorities separated by <c>-</c> = 10 characters (MAX) each.
    /// </remarks>
    public static readonly int MaxSidStringLength = SID_MAX_LENGTH;

    private readonly string _rawString;
    private string? _ldapString;

    /// <summary>
    /// The string representation of the SID in Security Descriptor Definition Language
    /// (SDDL) format.
    /// </summary>
    public string Value => _rawString;

    /// <summary>
    /// Creates a new instance of <see cref="SidString"/> from a string in the Security Descriptor
    /// Definition Language (SDDL) format.
    /// </summary>
    /// <param name="sddlForm">
    /// The string representation of the SID in Security Descriptor Definition Language (SDDL) format.
    /// </param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    public SidString(string sddlForm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sddlForm);
        if (sddlForm.Length > SID_MAX_LENGTH)
        {
            throw new ArgumentException("The length of the SID string is oo long.", nameof(sddlForm));
        }
        else if (!sddlForm.AsSpan().StartsWith(['S', '-'], StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The SID string does not start with 'S-'.", nameof(sddlForm));
        }

        _rawString = sddlForm.ToUpperInvariant();
    }
    /// <summary>
    /// Creates a new instance of <see cref="SidString"/> from a binary form of a SID.
    /// </summary>
    /// <param name="binaryForm"></param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if the length of <paramref name="binaryForm"/> is less than 8.
    /// </exception>
    public SidString(byte[] binaryForm)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(binaryForm.Length, 8, nameof(binaryForm));
        Span<char> span = stackalloc char[SID_MAX_LENGTH];
        int written = FormatSpan(span, binaryForm);
        _rawString = new string(span.Slice(0, written));
    }

    public bool Equals([NotNullWhen(true)] SidString? other)
    {
        if (RefEqualsOrNull(this, other, out bool result))
        {
            return result;
        }

        return StringComparer.OrdinalIgnoreCase.Equals(_rawString, other._rawString);
    }
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
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(_rawString);
    }
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

    [SupportedOSPlatform("WINDOWS")]
    public string ToLdapString()
    {
        return _ldapString ??= CreateLdapString(new SecurityIdentifier(_rawString));
    }
    [SupportedOSPlatform("WINDOWS")]
    string IFormattable.ToString(string? format, IFormatProvider? provider)
    {
        return this.ToLdapString();
    }
    [SupportedOSPlatform("WINDOWS")]
    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
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
    [SupportedOSPlatform("WINDOWS")]
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return this.TryFormat(destination, out charsWritten);
    }

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
    private static char GetHexValue(int i)
    {
        return i < 10
            ? (char)(i + '0')
            : (char)(i - 10 + 'A');
    }

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
