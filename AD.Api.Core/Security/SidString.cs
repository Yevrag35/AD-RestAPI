using AD.Api.Strings.Spans;
using System.Buffers;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace AD.Api.Core.Security
{
    [SupportedOSPlatform("WINDOWS")]
    public sealed class SidString : IEquatable<SidString>
    {
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
    }
}

