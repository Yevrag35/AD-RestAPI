using AD.Api.Core.Ldap;
using AD.Api.Spans;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization.Json;

public sealed class DistinguishedNameConverter : JsonConverter<DistinguishedName>
{
    public override DistinguishedName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return ParseFromSpan(ref reader);

            case JsonTokenType.Null:
                return DistinguishedName.Empty;

            default:
                throw new JsonException("Invalid JSON token type.", new FormatException("Distinguished names must be strings."));
        }
    }

    private static DistinguishedName ParseFromSpan(ref Utf8JsonReader reader)
    {
        int length = Encoding.UTF8.GetMaxCharCount(reader.ValueSpan.Length);
        bool isRented = false;
        char[]? array = null;

        scoped Span<char> span = length <= 256
            ? stackalloc char[length]
            : SpanExtensions.RentArray(in length, ref isRented, ref array);

        int written = Encoding.UTF8.GetChars(reader.ValueSpan, span);
        span = span.Slice(0, written);
        if (!DistinguishedName.TryCountNumberOfRelativeNames(span, out int count))
        {
            return DistinguishedName.Empty;
        }

        RelativeName[] buffer = ArrayPool<RelativeName>.Shared.Rent(count);
        if (!DistinguishedName.TrySplit(span, buffer.AsSpan(0, count), out int namesWritten))
        {
            ArrayPool<RelativeName>.Shared.Return(buffer);
            return DistinguishedName.Empty;
        }

        DistinguishedName result = new(buffer.AsSpan(0, namesWritten));
        if (isRented)
        {
            ArrayPool<char>.Shared.Return(array!);
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, DistinguishedName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}