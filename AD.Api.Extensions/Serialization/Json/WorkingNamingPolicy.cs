using AD.Api.Exceptions;
using System.Text;
using System.Text.Json;

namespace AD.Api.Serialization.Json
{
    public readonly ref struct WorkingNamingPolicy
    {
        readonly JsonSpanCamelCaseNamingPolicy? _spanPolicy;
        readonly bool _usePolicy;

        [MemberNotNullWhen(true, nameof(Policy))]
        public readonly bool HasPolicy { get; }
        [MemberNotNullWhen(true, nameof(Policy), nameof(_spanPolicy))]
        public readonly bool IsSpanPolicy { get; }
        public readonly JsonNamingPolicy? Policy;

        public WorkingNamingPolicy(JsonSerializerOptions? options, bool useNamingPolicy = true)
        {
            JsonNamingPolicy? pol = options?.PropertyNamingPolicy;
            _usePolicy = useNamingPolicy;
            bool hasPol = options is not null;
            if (hasPol && pol is JsonSpanCamelCaseNamingPolicy spanPolicy)
            {
                _spanPolicy = spanPolicy;
                this.IsSpanPolicy = true;
            }

            this.HasPolicy = hasPol;
            Policy = pol;
        }

        public readonly void WritePropertyName(Utf8JsonWriter writer, string propertyName)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName);
            if (_usePolicy && this.HasPolicy)
            {
                if (this.IsSpanPolicy)
                {
                    WriteCharSpan(writer, _spanPolicy, propertyName);
                    return;
                }

                propertyName = Policy.ConvertName(propertyName);
            }

            writer.WritePropertyName(propertyName);
        }
        public readonly void WritePropertyName(Utf8JsonWriter writer, ReadOnlySpan<char> propertyNameSpan)
        {
            EmptyStructException.ThrowIf(propertyNameSpan.IsEmpty, typeof(ReadOnlySpan<char>));
            if (_usePolicy && this.HasPolicy)
            {
                if (this.IsSpanPolicy)
                {
                    WriteCharSpan(writer, _spanPolicy, propertyNameSpan);
                    return;
                }

                propertyNameSpan = Policy.ConvertName(propertyNameSpan.ToString());
                Debug.Fail("An allocation happened here ^");
            }

            writer.WritePropertyName(propertyNameSpan);
        }
        public readonly void WritePropertyName(Utf8JsonWriter writer, ReadOnlySpan<byte> propertyName)
        {
            EmptyStructException.ThrowIf(propertyName.IsEmpty, typeof(ReadOnlySpan<byte>));
            if (!_usePolicy || !this.HasPolicy)
            {
                writer.WritePropertyName(propertyName);
                return;
            }
            else if (this.IsSpanPolicy)
            {
                Span<byte> tempSpan = stackalloc byte[propertyName.Length];
                propertyName.CopyTo(tempSpan);
                _spanPolicy.ConvertSpan(tempSpan);
                writer.WritePropertyName(tempSpan);
                return;
            }

            int length = Encoding.UTF8.GetCharCount(propertyName);
            Span<char> chars = stackalloc char[length];
            int written = Encoding.UTF8.GetChars(propertyName, chars);
            writer.WritePropertyName(Policy.ConvertName(chars.Slice(0, written).ToString()));
            Debug.Fail("An allocation happened here ^");
        }

        private static string AllocateString(ReadOnlySpan<byte> utf8Text, JsonNamingPolicy policy)
        {
            Span<char> chars = stackalloc char[utf8Text.Length];
            int written = Encoding.UTF8.GetChars(utf8Text, chars);

            string s = policy.ConvertName(chars.Slice(0, written).ToString());
            Debug.Fail("An allocation happened here ^");

            return s;
        }
        private static void WriteCharSpan(Utf8JsonWriter writer, JsonSpanCamelCaseNamingPolicy spanPolicy, ReadOnlySpan<char> propertyName)
        {
            Span<char> span = stackalloc char[propertyName.Length];
            spanPolicy.ConvertSpan(span);
            writer.WritePropertyName(span);
        }
    }
}

