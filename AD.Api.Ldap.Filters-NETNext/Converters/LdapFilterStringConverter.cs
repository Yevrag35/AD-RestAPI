using AD.Api.Ldap.Filters.Filters;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Ldap.Filters.Converters
{
    public sealed class LdapFilterStringConverter : JsonConverter<LdapFilterString>
    {
        static readonly byte[] _isEncodedName = "isEncoded"u8.ToArray();
        static readonly byte[] _valueName = "value"u8.ToArray();

        public override LdapFilterString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return ReadAsString(ref reader, options);
            }
            else
            {
                return ReadAsObject(ref reader, options);
            }
        }

        private static LdapFilterString ReadAsString(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Span<char> chars = stackalloc char[reader.ValueSpan.Length];
            int written = Encoding.UTF8.GetChars(reader.ValueSpan, chars);

            chars = chars.Slice(0, written);

            if (!HasCorrectParentheses(chars))
            {
                ThrowJsonEx(chars);
            }

            return new LdapFilterString
            {
                IsEncoded = false,
                Value = new string(chars),
            };
        }

        private static LdapFilterString ReadAsObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject || !reader.Read())
            {
                ThrowJsonEx();
            }

            var filter = new LdapFilterString { IsEncoded = false, Value = string.Empty };
            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(_isEncodedName) && reader.Read())
                {
                    filter.IsEncoded = reader.GetBoolean();
                }
                else if (reader.ValueTextEquals(_valueName) && reader.Read())
                {
                    filter.Value = reader.GetString() ?? string.Empty;
                }

                reader.Read();
            }

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                reader.Read();
            }

            return filter;
        }

        [DoesNotReturn]
        private static void ThrowJsonEx(ReadOnlySpan<char> filter = default)
        {
            string msg = "Invalid filter string provided.";
            if (!filter.IsWhiteSpace())
            {
                msg = msg + " " + filter.ToString();
            }

            throw new JsonException(msg);
        }

        public override void Write(Utf8JsonWriter writer, LdapFilterString value, JsonSerializerOptions options)
        {
            JsonNamingPolicy? policy = options.PropertyNamingPolicy;

            if (value.IsEncoded)
            {
                writer.WriteStartObject();
                writer.WriteBoolean(GetName(policy, nameof(LdapFilterString.IsEncoded)), value.IsEncoded);
                writer.WriteString(GetName(policy, nameof(LdapFilterString.Value)), value.Value);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStringValue(value.Value);
            }
        }

        private static string GetName(JsonNamingPolicy? policy, string name)
        {
            return policy?.ConvertName(name) ?? name;
        }

        private static bool HasCorrectParentheses(ReadOnlySpan<char> span)
        {
            int openCount = 0;
            int closedCount = 0;
            for (int i = 0; i < span.Length; i++)
            {
                switch (span[i])
                {
                    case '(':
                        openCount++;
                        goto default;

                    case ')':
                        closedCount++;
                        goto default;

                    default:
                        continue;
                }
            }

            return openCount == closedCount;
        }
    }
}
