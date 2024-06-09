using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AD.Api.Ldap.Filters.Filters
{
    public sealed record LdapFilterString
    {
        private string? _decodedValue;

        [MemberNotNullWhen(true, nameof(_decodedValue))]
        private bool HasBeenDecoded { get; set; }

        public required bool IsEncoded { get; set; }

        public required string Value { get; set; }

        public string GetString()
        {
            if (this.HasBeenDecoded)
            {
                return _decodedValue;
            }

            if (!this.IsEncoded)
            {
                _decodedValue = !string.IsNullOrWhiteSpace(this.Value) ? this.Value : string.Empty;
                this.HasBeenDecoded = true;
                return _decodedValue;
            }
            else if (string.IsNullOrWhiteSpace(this.Value))
            {
                _decodedValue = string.Empty;
                this.HasBeenDecoded = true;
                return _decodedValue;
            }

            int length = GetByteSpanLength(this.Value);
            Span<byte> bytes = stackalloc byte[length];

            _ = Convert.TryFromBase64String(this.Value, bytes, out int written);
            _decodedValue = Encoding.UTF8.GetString(bytes.Slice(0, written));
            this.HasBeenDecoded = true;
            return _decodedValue;
        }

        private static int GetByteSpanLength(ReadOnlySpan<char> base64Span)
        {
            int base64InputLength = base64Span.Length;
            int maxByteOutputLength = (3 * base64InputLength) / 4;

            if (base64Span[base64InputLength - 1] == '=')
            {
                maxByteOutputLength--;
            }

            if (base64Span[base64InputLength - 2] == '=')
            {
                maxByteOutputLength--;
            }

            return maxByteOutputLength;
        }
    }
}
