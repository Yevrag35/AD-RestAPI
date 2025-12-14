namespace AD.Api.Serialization.Json;

/// <summary>
/// Provides a naming policy for converting JSON property names to camel case using spans.
/// </summary>
public sealed class JsonSpanCamelCaseNamingPolicy : JsonNamingPolicy
{
	private static readonly JsonNamingPolicy s_camelCase = CamelCase;
	/// <summary>
	/// Gets the singleton instance of the <see cref="JsonSpanCamelCaseNamingPolicy"/> class.
	/// </summary>
	public static readonly JsonSpanCamelCaseNamingPolicy SpanPolicy = new();

	/// <summary>
	/// Converts the specified name to camel case.
	/// </summary>
	/// <param name="name">The name to convert.</param>
	/// <returns>The camel case version of the name.</returns>
	public override string ConvertName(string name)
	{
		return s_camelCase.ConvertName(name);
	}

	/// <summary>
	/// Converts the specified span of characters to camel case.
	/// </summary>
	/// <param name="span">The span of characters to convert.</param>
	/// <returns>The camel case version of the span.</returns>
	public Span<char> ConvertSpan(Span<char> span)
	{
		FixCasing(span);
		return span;
	}

	/// <summary>
	/// Converts the specified span of UTF-8 bytes to camel case.
	/// </summary>
	/// <param name="utf8Text">The span of UTF-8 bytes to convert.</param>
	/// <returns>The camel case version of the span.</returns>
	public Span<byte> ConvertSpan(Span<byte> utf8Text)
	{
		FixCasing(utf8Text);
		return utf8Text;
	}

	private static void FixCasing(Span<char> chars)
	{
		for (int i = 0; i < chars.Length; i++)
		{
			if (i == 1 && !char.IsUpper(chars[i]))
			{
				break;
			}

			int next = i + 1;

			// Stop when next char is already lowercase.
			if (i > 0 && next < chars.Length && !char.IsUpper(chars[next]))
			{
				// If the next char is a space, lowercase current char before exiting.
				if (chars[next] == ' ')
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
		if (utf8Bytes.IsEmpty)
		{
			return;
		}

		// Decode the first rune from the span.
		var status = Rune.DecodeFromUtf8(utf8Bytes, out Rune firstRune, out int bytesConsumed);
		if (status != OperationStatus.Done)
		{
			throw new JsonException("Invalid UTF-8 sequence.");
		}

		// Encode the lowercase rune back into the span.
		Span<byte> tempSlice = utf8Bytes.Slice(0, bytesConsumed);

		// Convert the first rune to lowercase.
		var lowerRune = Rune.ToLowerInvariant(firstRune);

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
