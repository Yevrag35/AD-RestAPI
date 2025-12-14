using AD.Api.Enums;
using AD.Api.Exceptions;
using AD.Api.Statics;

namespace AD.Api.Core.Ldap.Filters;

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
		try
		{
			builder.Append(prefix);
			builder.Append(expected);
			builder.Append('\'');

			CreateTokenTypeString(expectedTypes, enumStrings, ref builder);

			builder.Append(['\'', ';', ' ']);
			builder.Append(actual);
			builder.Append('\'');
			builder.Append(enumStrings[current]);
			builder.Append('\'');

			return builder.ToString();
		}
		finally
		{
			builder.Dispose();
		}
	}
	private static void CreateTokenTypeString(scoped Span<FilterTokenType> expectedTypes, IEnumStrings<FilterTokenType> enumStrings, ref SpanStringBuilder builder)
	{
		const int MAX_STACKALLOC = 256;

		if (expectedTypes.Length == 1)
		{
			builder.Append(enumStrings[expectedTypes[0]]);
		}

		Span<char> joinBy = [CharConstants.COMMA, CharConstants.SPACE];
		int position = 0;

		int totalLength = enumStrings.TotalNameLength;
		using (var buffer = RentedBuffer.Rent<char>(
			totalLength <= MAX_STACKALLOC
				? stackalloc char[totalLength]
				: totalLength))
		{
			for (int i = 0; i < expectedTypes.Length - 1; i++)
			{
				position = enumStrings[expectedTypes[i]].CopyToSlice(buffer.Span, position);
				position = joinBy.CopyToSlice(buffer.Span, position);
			}

			position = enumStrings[expectedTypes[^1]].CopyToSlice(buffer.Span, position);

			builder.Append(buffer[..position]);
		}
	}
}

