using AD.Api.Extensions.Comparisons;
using AD.Api.Spans;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using AD.Api.Strings.Spans;
using System.Runtime.CompilerServices;

namespace AD.Api.Core.Security;

/// <summary>
/// A cross-platform class that represents a security identifier (SID) string and its binary form.
/// </summary>
/// <remarks>
/// Like the Windows-only 'SecurityIdentifier' implementation, this class is designed to be immutable.
/// </remarks>
public sealed class SidString : IComparable<SidString>, IEquatable<SidString>, ISpanFormattable
{
    private static readonly char L_FORMAT = 'L';
    private const int SID_CHAR_MAX_LENGTH = 189;

    #region PUBLIC CONSTANTS
    /// <summary>
    /// Minimum length of a binary representation of a SID
    /// </summary>
    /// <remarks>
    /// Revision (1) + Identity Authority (6) + Sub-Authority Count (1) = <c>8</c>
    /// </remarks>
    public const int MinBinaryLength = 8;
    /// <summary>
    /// The maximum value for the identifier authority in a SID.
    /// </summary>
    /// <remarks>
    /// Identifier authorities must be at most six bytes long.
    /// </remarks>
    /// <value>
    /// <c>281474976710655</c>
    /// </value>
    public const long MaxIdentifierAuthority = 0xFFFFFFFFFFFF;
    /// <summary>
    /// Maximum number of subauthorities in a SID.
    /// </summary>
    public const int MaxSubAuthorities = 15;
    /// <summary>
    /// Maximum length of a binary representation of a SID.
    /// </summary>
    /// <remarks>
    /// <see cref="MinBinaryLength"/> + (<see cref="MaxSubAuthorities"/> * 4) = <c>68</c>
    /// <br/>4 bytes for each sub-authority.
    /// </remarks>
    /// <value>
    /// <c>68</c>
    /// </value>
    public static readonly int MaxBinaryLength = MinBinaryLength + (MaxSubAuthorities * 4);
    
    /// <summary>
    /// The format specifier for the LDAP string representation of the SID.
    /// </summary>
    /// <remarks>
    /// Used for constructing LDAP queries with the SID as a filter.
    /// </remarks>
    /// <returns>
    /// A <see cref="ReadOnlySpan{T}"/> consisting of one <see cref="char"/> element: <c>L</c>
    /// </returns>
    public static ReadOnlySpan<char> LdapFormat => new(in L_FORMAT);
    /// <summary>
    /// The format specifier for the Security Descriptor Definition Language (SDDL) string representation of the SID 
    /// which is default.
    /// </summary>
    /// <returns>
    /// An empty <see cref="ReadOnlySpan{T}"/>.
    /// </returns>
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
    public static readonly int MaxSidStringLength = SID_CHAR_MAX_LENGTH;
    /// <summary>
    /// The minimum number of characters in any given SID string in Security Descriptor Definition Language (SDDL) 
    /// format.
    /// </summary>
    public const int MinSidStringLength = 7;

    #endregion

    private readonly byte[] _binaryForm;
    private readonly string _rawString;
    private string? _ldapString;

    #region PUBLIC PROPERTIES
    /// <summary>
    /// Gets the length, in bytes, of the <see cref="SidString"/> object's binary representation.
    /// </summary>
    /// <returns>
    /// The length, in bytes, of the SID represented by this <see cref="SidString"/> object.
    /// </returns>
    public int BinaryLength => _binaryForm.Length;
    /// <summary>
    /// Gets the number of characters that would make up the LDAP filter string representation of this 
    /// <see cref="SidString"/>.
    /// </summary>
    /// <returns>
    /// The number of <see cref="char"/> elements will make up the LDAP filter string representation of this
    /// <see cref="SidString"/> object which is equal to 3 times the <see cref="BinaryLength"/> value.
    /// </returns>
    public int LdapStringLength => this.BinaryLength * 3;

    /// <summary>
    /// Gets the string representation of the SID in Security Descriptor Definition Language (SDDL) format.
    /// </summary>
    /// <returns>
    /// An uppercase SDDL <see cref="string"/> for the SID represented by this <see cref="SidString"/> object.
    /// </returns>
    public string Value => _rawString;

    #endregion

    #region CONSTRUCTORS

    /// <summary>
    /// Initializes a new instance of the <see cref="SidString"/> class from the pre-validated character and 
    /// binary values.
    /// </summary>
    /// <remarks>
    /// Used only by the <see cref="TryParse(ReadOnlySpan{char}, out SidString?)"/> method.
    /// </remarks>
    /// <param name="sddlFormSpan">The pre-validated SDDL form of the SID.</param>
    /// <param name="binaryForm">The pre-validated binary form of the SID.</param>
    private SidString(ReadOnlySpan<char> sddlFormSpan, ReadOnlySpan<byte> binaryForm)
    {
        scoped ReadOnlySpan<char> final = sddlFormSpan;
        if ('S' != final[0])
        {
            Span<char> toUpper = stackalloc char[final.Length];
            final.ToUpperInvariant(toUpper);
            final = toUpper;
        }

        _rawString = final.ToString();
        _binaryForm = binaryForm.ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SidString"/> class from a string in the Security Descriptor 
    /// Definition Language (SDDL) format.
    /// </summary>
    /// <param name="sddlForm">
    /// The string representation of the SID in Security Descriptor Definition Language (SDDL) format.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="sddlForm"/> is empty or does not start with 'S-'.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="sddlForm"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The length of <paramref name="sddlForm"/> is greater than <see cref="MaxSidStringLength"/> or 
    /// less than <see cref="MinSidStringLength"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// The SID string is not in a valid format and could not be converted to a binary form.
    /// </exception>
    public SidString(string sddlForm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sddlForm);
        ThrowIfLengthNotInRange(sddlForm);

        if (!sddlForm.AsSpan().StartsWith(['S', CharConstants.HYPHEN], StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The SID string does not start with 'S-'.", nameof(sddlForm));
        }
        
        Span<byte> bytes = stackalloc byte[MaxBinaryLength];
        if (!TryConvertSidStringToBinary(sddlForm, bytes, out int written))
        {
            throw new FormatException($"{nameof(sddlForm)}: The SID string '{sddlForm}' is not in a valid format.");
        }

        _rawString = sddlForm.ToUpperInvariant();
        _binaryForm = bytes.Slice(0, written).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SidString"/> class from a binary form of a SID.
    /// </summary>
    /// <param name="binaryForm">The binary representation of the SID.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The length of <paramref name="binaryForm"/> is less than <see cref="MinBinaryLength"/> or greater than
    /// <see cref="MaxBinaryLength"/>.
    /// </exception>
    public SidString(ReadOnlySpan<byte> binaryForm)
    {
        ThrowIfLengthNotInRange(binaryForm);

        Span<char> span = stackalloc char[SID_CHAR_MAX_LENGTH];
        int written = FormatSpan(span, binaryForm);
        ref char first = ref span[0];
        first = char.ToUpperInvariant(first);

        _rawString = new string(span.Slice(0, written));

        _binaryForm = new byte[binaryForm.Length];
        binaryForm.CopyTo(_binaryForm);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="SidString"/> class from a binary form of a SID.
    /// </summary>
    /// <param name="binaryForm">The binary representation of the SID.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="binaryForm"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The length of <paramref name="binaryForm"/> is less than <see cref="MinBinaryLength"/> or greater than
    /// <see cref="MaxBinaryLength"/>.
    /// </exception>
    public SidString(byte[] binaryForm, bool copyArray)
    {
        ArgumentNullException.ThrowIfNull(binaryForm);
        ThrowIfLengthNotInRange(binaryForm);

        Span<char> span = stackalloc char[SID_CHAR_MAX_LENGTH];
        int written = FormatSpan(span, binaryForm);
        ref char first = ref span[0];
        first = char.ToUpperInvariant(first);

        _rawString = new string(span.Slice(0, written));

        if (!copyArray)
        {
            _binaryForm = binaryForm;
        }

        _binaryForm = new byte[binaryForm.Length];
        binaryForm.AsSpan().CopyTo(_binaryForm);
    }

    #endregion

    #region COMPARISONS AND EQUALITY

    public int CompareTo(SidString? other)
    {
        if (this.RefEqualsOrNull(other, out bool result))
        {
            return result ? 0 : -1;
        }

        return StringComparer.OrdinalIgnoreCase.Compare(_rawString, other._rawString);
    }

    /// <summary>
    /// Indicates whether the current SID string is equal to another SID string.
    /// </summary>
    /// <param name="other">The SID string to compare to this instance.</param>
    /// <returns><see langword="true"/> if the current SID string is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals([NotNullWhen(true)] SidString? other)
    {
        if (!this.RefEqualsOrNull(other, out bool result))
        {
            result = StringComparer.OrdinalIgnoreCase.Equals(_rawString, other._rawString);
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current SID string.
    /// </summary>
    /// <param name="obj">The object to compare with the current SID string.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current SID string; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is SidString other)
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

    #endregion

    /// <summary>
    /// Formats the binary form of a SID into its string representation.
    /// </summary>
    /// <param name="destination">The destination span to write the formatted SID string.</param>
    /// <param name="sidBytes">The binary representation of the SID.</param>
    /// <returns>The number of characters written to the destination span.</returns>
    public static int FormatSpan(Span<char> destination, ReadOnlySpan<byte> sidBytes)
    {
        if (!IsByteLengthInRange(sidBytes))
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
    /// Gets the LDAP string representation of the SID.
    /// </summary>
    /// <returns>The LDAP string representation of the SID.</returns>
    public string ToLdapString()
    {
        return _ldapString ??= CreateLdapString(this);
    }

    /// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
    string IFormattable.ToString(string? format, IFormatProvider? provider)
    {
        return this.ToLdapString();
    }

    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default)
    {
        if (!format.IsEmpty && format.Equals(LdapFormat, StringComparison.OrdinalIgnoreCase))
        {
            if (_ldapString is not null)
            {
                return _ldapString.TryCopyTo(destination, out charsWritten);
            }

            SpanStringBuilder builder = new(stackalloc char[destination.Length]);

            WriteLdapToSpan(ref builder, this);
            if (builder.Length > destination.Length)
            {
                charsWritten = 0;
                builder.Dispose();
                return false;
            }

            charsWritten = builder.CopyTo(destination);
            builder.Dispose();

            return charsWritten > 0;
        }

        return _rawString.TryCopyTo(destination, out charsWritten);
    }

    /// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return this.TryFormat(destination, out charsWritten);
    }
    /// <summary>
    /// Attempts to parse the specified read-only span of characters into a fully-formed <see cref="SidString"/> object.
    /// </summary>
    /// <param name="value">The read-only span to parse.</param>
    /// <param name="sid">
    /// When this method returns, contains the <see cref="SidString"/> object parsed from the read-only span, 
    /// if the parse operation was successful; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the parse operation was successful; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out SidString? sid)
    {
        value = value.Trim();
        if (value.IsWhiteSpace() || !IsCharLengthInRange(value))
        {
            sid = null;
            return false;
        }

        Span<byte> buffer = stackalloc byte[MaxBinaryLength];
        if (!TryConvertSidStringToBinary(value, buffer, out int written))
        {
            sid = null;
            return false;
        }

        sid = new SidString(value, buffer.Slice(0, written));
        return true;
    }

    /// <summary>
    /// Creates an LDAP string representation of the specified <see cref="SidString"/>.
    /// </summary>
    /// <param name="sid">The <see cref="SidString"/> to create the LDAP string for.</param>
    /// <returns>The LDAP string representation of the specified <see cref="SidString"/>.</returns>
    private static string CreateLdapString(SidString sid)
    {
        SpanStringBuilder builder = new(stackalloc char[sid.LdapStringLength]);

        FormatByteArrayToSpan(sid._binaryForm, ref builder);

        string result = builder.Build();

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

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsByteLengthInRange(ReadOnlySpan<byte> value)
    {
        return value.Length >= MinBinaryLength && value.Length <= MaxBinaryLength;
    }
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCharLengthInRange(ReadOnlySpan<char> value)
    {
        return value.Length >= MinSidStringLength && value.Length <= MaxSidStringLength;
    }

    [DebuggerStepThrough]
    private static void ThrowIfLengthNotInRange(ReadOnlySpan<byte> value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!IsByteLengthInRange(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "The given byte array's length is not within the acceptable range.");
        }
    }
    [DebuggerStepThrough]
    private static void ThrowIfLengthNotInRange(ReadOnlySpan<char> value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!IsCharLengthInRange(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "The given string's length is not within the acceptable range.");
        }
    }

    private static bool TryConvertSidStringToBinary(ReadOnlySpan<char> sidString, Span<byte> buffer, out int bytesWritten)
    {
        SpanCharArray parts = new(sidString.Length, '-');
        parts.AddRange(sidString, ['-']);

        if (parts.Count < 4 || !parts[0].Equals(['S'], StringComparison.OrdinalIgnoreCase) ||
            !parts[1].Equals(['1'], StringComparison.Ordinal))
        {
            bytesWritten = 0;
            parts.Dispose();
            return false;
        }

        int subAuthorityCount = parts.Count - 3;
        bytesWritten = 0;
        buffer[bytesWritten++] = 1; // Revision
        buffer[bytesWritten++] = (byte)subAuthorityCount; // Sub-authority count

        // Authority (next 6 bytes)
        if (!ulong.TryParse(parts[2], out ulong identifierAuthority))
        {
            parts.Dispose();
            return false;
        }

        for (int i = 0; i < 6; i++)
        {
            buffer[7 - i] = (byte)(identifierAuthority >> (8 * i));
        }

        bytesWritten += 6;
        // Sub-authorities (next 4 bytes each)
        for (int i = 0; i < subAuthorityCount; i++)
        {
            uint subAuthority = uint.Parse(parts[i + 3]);
            for (int j = 0; j < 4; j++)
            {
                buffer[8 + i * 4 + j] = (byte)(subAuthority >> (8 * j));
            }

            bytesWritten += 4;
        }

        parts.Dispose();
        return true;
    }

    /// <summary>
    /// Writes the LDAP string representation of the specified <see cref="SidString"/> to the destination span.
    /// </summary>
    /// <param name="destination">The destination span to write the LDAP string to.</param>
    /// <param name="sid">The <see cref="SidString"/> to create the LDAP string for.</param>
    /// <param name="written">The number of characters written to the destination span.</param>
    private static void WriteLdapToSpan(ref SpanStringBuilder builder, SidString sid)
    {
        FormatByteArrayToSpan(sid._binaryForm, ref builder);
    }
}
