using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Enums;
using System.Text;

namespace AD.Api.Core.Serialization.Json.Converters;

public sealed class UserAccountControlConverter : JsonConverter<UserAccountControl>
{
	private const int MAX_STACK = 256;

	public override UserAccountControl Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number || reader.TokenType != JsonTokenType.String)
		{
			throw new JsonException($"Unexpected token parsing UserAccountControl. Expected Number or String, got {reader.TokenType}.");
		}
		
		if (reader.TokenType == JsonTokenType.Number && int.TryParse(reader.ValueSpan, out int result))
		{
			return (UserAccountControl)result;
		}

		int maxLength = Encoding.UTF8.GetMaxCharCount(reader.ValueSpan.Length);
		using (var buffer = RentedBuffer.Rent<char>(
			maxLength <= MAX_STACK
				? stackalloc char[maxLength]
				: maxLength))
		{
			int charCount = Encoding.UTF8.GetChars(reader.ValueSpan, buffer.Span);
			if (Enum.TryParse<UserAccountControl>(buffer[..charCount], ignoreCase: true, out var enumValue))
			{
				return enumValue;
			}
		}

		throw new JsonException($"Unable to convert \"{reader.GetString()}\" to UserAccountControl.");
	}

	public override void Write(Utf8JsonWriter writer, UserAccountControl value, JsonSerializerOptions options)
	{
		Span<char> buffer = stackalloc char[UserAccountControlEnumToStringExtensions.GetMaxToStringFastLength()];
		int written = value.CopyTo(buffer);
		writer.WriteStringValue(buffer[..written]);
	}
}
