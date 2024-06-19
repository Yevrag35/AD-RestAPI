using System.Buffers;
using System.Text;
using System.Text.Json;

namespace AD.Api.Serialization.Json
{
    public class JsonSpanCamelCaseNamingPolicy : JsonNamingPolicy
    {
        public static readonly JsonNamingPolicy SpanPolicy = new JsonSpanCamelCaseNamingPolicy();

        public JsonSpanCamelCaseNamingPolicy()
        {
        }

        public void ConvertSpan(Span<char> span)
        {
            if (span.IsEmpty)
            {
                return;
            }

            this.ConvertSpanCore(span);
        }
        public void ConvertSpan(Span<byte> utf8Text)
        {
            if (utf8Text.IsEmpty)
            {
                return;
            }

            this.ConvertSpanCore(utf8Text);
        }
        public sealed override string ConvertName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            return string.Create(name.Length, (@this: this, name), (chars, state) =>
            {
                state.name.CopyTo(chars);
                state.@this.ConvertSpanCore(chars);
            });
        }

        protected virtual void ConvertSpanCore(Span<char> span)
        {
            FixCasing(span);
        }
        protected virtual void ConvertSpanCore(Span<byte> utf8Text)
        {
            FixCasing(utf8Text);
        }

        private static void FixCasing(Span<char> chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);

                // Stop when next char is already lowercase.
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    // If the next char is a space, lowercase current char before exiting.
                    if (chars[i + 1] == ' ')
                    {
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }

                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }
        }
        private static void FixCasing(Span<byte> utf8Bytes)
        {
            // Decode the first rune from the span.
            var status = Rune.DecodeFromUtf8(utf8Bytes, out Rune firstRune, out int bytesConsumed);
            Debug.Assert(status == OperationStatus.Done);
            if (status != OperationStatus.Done)
            {
                throw new JsonException("Invalid UTF-8 sequence.");
            }

            // Encode the lowercase rune back into the span.
            Span<byte> tempSlice = utf8Bytes.Slice(0, bytesConsumed);

            // Convert the first rune to lowercase.
            Rune lowerRune = Rune.ToLowerInvariant(firstRune);

            // Check if a change is needed
            if (firstRune != lowerRune)
            {
                // Re-encode the lowercase rune back into the span.
                // Note: Encoding might not change byte count since TitleCase generally implies simple capital letters.
                int written = lowerRune.EncodeToUtf8(tempSlice);
                if (bytesConsumed != written)
                {
                    throw new JsonException("Unexpected change in byte length when converting to lowercase");
                }
            }
        }
    }
}

