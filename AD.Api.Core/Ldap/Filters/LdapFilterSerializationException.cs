using AD.Api.Enums;
using AD.Api.Exceptions;
using AD.Api.Spans;
using AD.Api.Statics;
using AD.Api.Strings.Spans;
using System.Buffers;

namespace AD.Api.Core.Ldap.Filters
{
    public sealed class LdapFilterSerializationException : AdApiException
    {
        public FilterTokenType TokenType { get; }

        public LdapFilterSerializationException(FilterTokenType currentType, scoped Span<FilterTokenType> expectedTypes, IEnumStrings<FilterTokenType> enumStrings)
            : base(CreateMessage(in currentType, expectedTypes, enumStrings))
        {
            this.TokenType = currentType;
        }

        private static string CreateMessage(in FilterTokenType current, scoped Span<FilterTokenType> expectedTypes, IEnumStrings<FilterTokenType> enumStrings)
        {
            ReadOnlySpan<char> prefix = Errors.Exception_LdapFilterType_Prefix;
            ReadOnlySpan<char> expected = Errors.Exception_LdapFilterType_Expected;
            ReadOnlySpan<char> actual = Errors.Exception_LdapFilterType_Actual;

            SpanStringBuilder builder = new(stackalloc char[256]);
            builder = builder.Append(prefix)
                             .Append(expected)
                             .Append('\'');

            CreateTokenTypeString(expectedTypes, enumStrings, ref builder);

            builder = builder.Append(['\'', ';', ' '])
                             .Append(actual)
                             .Append('\'')
                             .Append(enumStrings[current])
                             .Append('\'');

            return builder.Build();
        }
        private static void CreateTokenTypeString(scoped Span<FilterTokenType> expectedTypes, IEnumStrings<FilterTokenType> enumStrings, ref SpanStringBuilder builder)
        {
            if (expectedTypes.Length == 1)
            {
                builder = builder.Append(enumStrings[expectedTypes[0]]);
            }

            char[]? array = null;
            bool isRented = false;

            Span<char> span = enumStrings.TotalNameLength > 256
                ? SpanExtensions.RentArray(enumStrings.TotalNameLength, ref isRented, ref array)
                : stackalloc char[enumStrings.TotalNameLength];

            Span<char> joinBy = [CharConstants.COMMA, CharConstants.SPACE];
            int position = 0;

            for (int i = 0; i < expectedTypes.Length - 1; i++)
            {
                enumStrings[expectedTypes[i]].CopyToSlice(span, ref position);
                joinBy.CopyToSlice(span, ref position);
            }

            enumStrings[expectedTypes[^1]].CopyToSlice(span, ref position);

            builder = builder.Append(span.Slice(0, position));
            if (isRented)
            {
                ArrayPool<char>.Shared.Return(array!);
            }
        }
    }
}

