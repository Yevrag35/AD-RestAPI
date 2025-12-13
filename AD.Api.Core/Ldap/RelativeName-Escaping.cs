using AD.Api.Collections.Enumerators;
using AD.Api.Statics;

namespace AD.Api.Core.Ldap;

public readonly partial struct RelativeName
{
	/// <summary>
	/// Determines whether the specified span of characters is a valid relative distinguished name.
	/// </summary>
	/// <param name="value">The read-only span of characters to validate.</param>
	/// <returns>
	/// <see langword="true"/> if the specified value is a valid relative distinguished name; otherwise, <see langword="false"/>.
	/// </returns>
	/// <remarks>
	/// This method checks for leading and trailing whitespace, proper escaping of special characters, and other
	/// LDAP-specific rules for relative distinguished names.
	/// </remarks>
	public static bool IsValid(ReadOnlySpan<char> value)
	{
		return IsValid(value, out _);
	}

	private static bool IsValid(ReadOnlySpan<char> value, out int equalsIndex)
	{
		equalsIndex = -1;

		// Check if the value is empty or consists only of whitespace
		if (value.IsWhiteSpace())
		{
			return false;
		}

		// Check if the first character is a '#' (invalid start character)
		if (CharConstants.POUND == value[0])
		{
			return false;
		}
		// Check if the value ends with an unescaped space (invalid ending character)
		else if (CharConstants.SPACE == value[^1] && value.Length > 2 && !value.IsEscapedAt(value.Length - 1))
		{
			return false;
		}

		// Iterate through each character validating each in sequence.
		for (int i = 0; i < value.Length; i++)
		{
			ref readonly char c = ref value[i];

			// Check if the character is a non-standard escaped character and not properly escaped
			if (NonStandardEscapedChars.Contains(c) && !value.IsEscapedAt(in i))
			{
				return false;
			}

			// Perform specific checks based on the character type
			switch (c)
			{
				case CharConstants.EQUALS:
				{
					// Check if the equals sign is properly placed. Equals signs can only be escaped
					// when using their hex value (\3D) so we only check if the previous characters are
					// valid attribute names.
					// And there can only be 1.
					if (equalsIndex >= 0 || !IsProperEquals(value, in i))
					{
						return false;
					}

					equalsIndex = i;
					break;
				}

				case CharConstants.COMMA:
				{
					// Check if the comma is properly escaped
					if (!value.IsEscapedAt(in i))
					{
						return false;
					}

					break;
				}

				case CharConstants.BACKSLASH:
				{
					// Check if the backslash is being used to escape the next character or is itself escaped.
					if (!IsProperBackslash(value, i))
					{
						return false;
					}

					break;
				}

				default:
					break;
			}
		}

		// If all checks pass, the value is valid
		return true;
	}

	private static bool IsProperEquals(ReadOnlySpan<char> working, in int index)
	{
		if (index < 1 || index >= working.Length - 1)
		{
			return false;
		}

		return IsValidPrefixNoError(working.Slice(0, index));
	}
	private static bool IsValidPrefixNoError(ReadOnlySpan<char> working)
	{
		ArrayRefEnumerator<string> enumerator = new(_attributeValues.Keys.AsSpan());
		bool flag = false;
		while (enumerator.MoveNext(in flag))
		{
			flag = working.Equals(enumerator.Current.AsSpan(0, enumerator.Current.Length - 1), StringComparison.OrdinalIgnoreCase);
		}

		return flag;
	}
	private static bool IsProperBackslash(ReadOnlySpan<char> value, int index)
	{
		if (index == value.Length - 1)
		{
			return value.IsEscapedAt(in index);
		}

		ref readonly char nextChar = ref value[index + 1];

		return AllEscapedChars.Contains(nextChar) || value.IsEscapedAt(in index);
	}
}
