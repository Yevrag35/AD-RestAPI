using AD.Api.Strings.Extensions;
using AD.Api.Strings.Spans;
using System.Buffers;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace AD.Api.Core.Security
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class SidString : IEquatable<SidString>, ISpanFormattable
    {
        private static readonly char L_FORMAT = 'L';
        private const int SID_MAX_LENGTH = 189;
        public static ReadOnlySpan<char> LdapFormat => new(in L_FORMAT);
        public static ReadOnlySpan<char> SidFormat => default;

        /// <summary>
        /// 2+2+20+15×(1+10)=189 maximum number of characters in a SID string.
        /// </summary>
        /// <remarks>
        /// <c>S-</c> = 2 characters<br/>
        /// Revision: <c>1-</c> = 2 characters<br/>
        /// Identifier Authority = 20 characters (MAX)<br/>
        /// 15 Sub-authorities separated by <c>-</c> = 10 characters (MAX) each.
        /// </remarks>
        public static readonly int MaxSidStringLength = SID_MAX_LENGTH;

        private readonly SecurityIdentifier _sid;
        private readonly string _rawString;
        private string? _ldapString;

        public SecurityIdentifier Identifier => _sid;
        public string Value => _rawString;

        public SidString(string rawString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rawString);
            _sid = new SecurityIdentifier(rawString);
            _rawString = rawString;
        }
        public SidString(byte[] binaryForm)
        {
            _sid = new SecurityIdentifier(binaryForm, 0);
            _rawString = _sid.Value;
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

        public string ToLdapString()
        {
            return _ldapString ??= CreateLdapString(_sid);
        }
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

                WriteLdapToSpan(ref destination, _sid, out charsWritten);
                return charsWritten > 0;
            }

            return _sid.Value.TryCopyTo(destination, out charsWritten);
        }
        bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            return this.TryFormat(destination, out charsWritten);
        }
        
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
    }
}

