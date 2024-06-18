using AD.Api.Core.Security;
using AD.Api.Core.Serialization;
using AD.Api.Statics;
using AD.Api.Strings.Spans;
using System.Diagnostics;
using System.Text.Json;

namespace AD.Api.Mapping
{
    public static partial class AttributeSerialization
    {
        

        public static void WriteObjectSID(Utf8JsonWriter writer, ref readonly SerializationContext context)
        {
            if (context.Value is not byte[] sidBytes)
            {
                WriteNonByteSid(writer, context.Value, context.Options);
                return;
            }

            WriteSid(writer, sidBytes);
        }

        private static void WriteSid(Utf8JsonWriter writer, ReadOnlySpan<byte> sidBytes)
        {
            if (sidBytes.Length < 8 || sidBytes[0] != 1)
            {
                Debug.Fail("Invalid SID byte array.");
                writer.WriteNullValue();
                return;
            }

            int revision = sidBytes[0];
            int subAuthorityCount = sidBytes[1];
            long identifierAuthority = (long)sidBytes.Slice(2, 6)[0] << 40 |
                                       (long)sidBytes.Slice(2, 6)[1] << 32 |
                                       (long)sidBytes.Slice(2, 6)[2] << 24 |
                                       (long)sidBytes.Slice(2, 6)[3] << 16 |
                                       (long)sidBytes.Slice(2, 6)[4] << 8 |
                                       sidBytes.Slice(2, 6)[5];

            char separator = CharConstants.HYPHEN;
            SpanStringBuilder builder = new(stackalloc char[SidString.MaxSidStringLength]);
            builder = builder.Append(['S', separator])
                             .Append(revision)
                             .Append(separator)
                             .Append(identifierAuthority);

            for (int i = 0; i < subAuthorityCount; i++)
            {
                uint subAuth = BitConverter.ToUInt32(sidBytes.Slice(8 + i * 4, 4));
                int length = subAuth.GetLength() + 1;
                builder = builder.Append(length, subAuth, (chars, state) =>
                {
                    chars[0] = CharConstants.HYPHEN;
                    _ = state.TryFormat(chars.Slice(1), out _);
                });
            }

            writer.WriteStringValue(builder.AsSpan());
            builder.Dispose();
        }
        private static void WriteNonByteSid(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else if (value is string sidString)
            {
                writer.WriteStringValue(sidString);
            }
            else
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }
    }
}
