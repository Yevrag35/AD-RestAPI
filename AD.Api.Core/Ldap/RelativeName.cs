using AD.Api.Attributes;
using AD.Api.Attributes.Services;
using AD.Api.Enums;
using AD.Api.Validation;
using System.Collections.Frozen;
using System.ComponentModel;

namespace AD.Api.Core.Ldap;

/// <summary>
/// A struct representing one relative distinguished name that when combined and separated with commas forms
/// a fully-qualified distinguished name.
/// </summary>
/// <remarks>
/// Also known as a Relative Distinguished Name (RDN), a relative name is a single attribute-value pair that
/// represents a single attribute of an object in a directory.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
[DynamicDependencyRegistration]
[DebuggerDisplay(@"\{Type={AttributeType}; Value={Value}\}")]
public readonly partial struct RelativeName :
	ICanBeEmpty,
	IEquatable<RelativeName>,
	IEquatable<string>
{
	private const int DN_SPAN_LIMIT = 256;  // Maximum stackalloc length of a distinguished name.
	private const int MINIMUM_NAME_INDEX = 2;   // Minimum index for a valid attributed name.

	/// <summary>
	/// A read-only dictionary of the <see cref="RelativeNameType"/> attribute values and their LDAP string
	/// representations.
	/// </summary>
	/// <remarks>
	/// The <see cref="string"/> values include the trailing, separating equals sign, but do not include the preceding
	/// comma. An example:
	/// <code>
	/// { RelativeNameType.CommonName, "CN=" }
	/// </code>
	/// </remarks>
	public static readonly IEnumValues<RelativeNameType, BackendValueAttribute, string> AttributeStrings;

	private static readonly FrozenDictionary<string, RelativeNameType> _attributeValues;
	public static readonly RelativeName Empty;

	/// <summary>
	/// Characters that need to be escaped in a distinguished name.
	/// </summary>
	public static readonly SearchValues<char> NonStandardEscapedChars;
	public static readonly SearchValues<char> AllEscapedChars;
	public static readonly SearchValues<char> UniqueAttributeChars;

	static RelativeName()
	{
		Span<char> allEscaped = ['\\', ',', '+', '>', '<', ';', '"']; // except '=', which is even more special.
		AllEscapedChars = SearchValues.Create(allEscaped);
		NonStandardEscapedChars = SearchValues.Create(allEscaped.Slice(2));
		AttributeStrings = EnumValues.Create<RelativeNameType, BackendValueAttribute, string>(freeze: true);
		Dictionary<string, RelativeNameType> valueDict = AttributeStrings
			.ToValueDictionary(StringComparer.OrdinalIgnoreCase);

		_attributeValues = FrozenDictionary.ToFrozenDictionary(valueDict, valueDict.Comparer);
		Empty = new(RelativeNameType.CommonName, string.Empty, -1);
		Span<char> chars = stackalloc char[AttributeStrings.ValueCount * 7];
		int count = 0;
		foreach (char c in _attributeValues.Keys.SelectMany(x => x).Distinct())
		{
			chars[count++] = c;
		}

		UniqueAttributeChars = SearchValues.Create(chars.Slice(0, count));
	}

	private readonly int _nameStartIndex;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly bool _notEmpty;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string? _value;

	/// <summary>
	/// Gets the attribute type of the relative name.
	/// </summary>
	public readonly RelativeNameType AttributeType { get; }

	/// <summary>
	/// Gets a value indicating whether this instance is empty.
	/// </summary>
	[MemberNotNullWhen(false, nameof(_value))]
	public readonly bool IsEmpty => !_notEmpty;

	/// <summary>
	/// Gets the string value of the relative name.
	/// </summary>
	public readonly string Value => _value ?? string.Empty;

	private RelativeName(RelativeNameType attributeType, string value, in int nameIndex)
	{
		this.AttributeType = attributeType;
		if (!string.IsNullOrWhiteSpace(value))
		{
			_value = value;
			_notEmpty = nameIndex >= 2 && nameIndex < value.Length;
			_nameStartIndex = nameIndex;
		}
		else
		{
			_value = string.Empty;
			_nameStartIndex = -1;
			_notEmpty = false;
		}
	}

	private static ReadOnlySpan<char> BuildPrefix(in RelativeNameType nameTypeIfNotPresent, Span<char> buffer, ReadOnlySpan<char> span, ref ReadOnlySpan<char> prefix)
	{
		prefix = AttributeStrings.GetValue(nameTypeIfNotPresent);
		prefix.CopyTo(buffer);
		span.CopyTo(buffer.Slice(prefix.Length));
		return buffer.Slice(0, span.Length + prefix.Length);
	}

	/// <summary>
	/// Creates a new <see cref="RelativeName"/> instance from the specified span of characters prepending the
	/// specified <see cref="RelativeNameType"/> prefix if it is not already present.
	/// </summary>
	/// <param name="value">The span of characters that makes up the relative name.</param>
	/// <param name="nameType">The type of relative name the instance will prepend.</param>
	/// <returns>
	/// A new <see cref="RelativeName"/> instance with the specified
	/// <paramref name="nameType"/> and <paramref name="value"/>.
	/// </returns>
	/// <exception cref="ArgumentException">The specified distinguished name is invalid.</exception>
	public static RelativeName Create(scoped ReadOnlySpan<char> value, RelativeNameType nameType)
	{
		if (value.IsWhiteSpace() || !AttributeStrings.TryGetValue(nameType, out string? prefix))
		{
			return Empty;
		}

		string v = value.ToString();
		if (!IsValid(value))
		{
			throw new ArgumentException("The specified distinguished name is invalid - make sure to escape any special characters.", nameof(value));
		}

		int index = prefix.Length;
		//value = EscapeChars(value, stackalloc char[value.Length * 2]);

		if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
		{
			return new(nameType, value.ToString(), in index);
		}

		Span<char> chars = stackalloc char[value.Length + prefix.Length];
		prefix.CopyTo(chars);
		value.CopyTo(chars.Slice(prefix.Length));

		return new(nameType, new string(chars), in index);
	}

	/// <summary>
	/// Gets the name portion of the relative distinguished name.
	/// </summary>
	/// <returns>A read-only span of characters representing the name.</returns>
	public ReadOnlySpan<char> GetName()
	{
		return _notEmpty ? _value.AsSpan(_nameStartIndex) : ReadOnlySpan<char>.Empty;
	}

	private static RelativeNameType GetRelativeNameType(ReadOnlySpan<char> prefix)
	{
		ThrowWhenInvalidPrefix(prefix);

		foreach (var kvp in _attributeValues)
		{
			if (prefix.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
			{
				return kvp.Value;
			}
		}

		Debug.Fail("This should be unreachable...");
		return RelativeNameType.CommonName;
	}

	private static bool IsValidPrefix(ReadOnlySpan<char> prefix, [NotNullWhen(false)] out ArgumentException? exception)
	{
		if (prefix.Length < 2 || prefix.Length > 7)
		{
			exception = new ArgumentException("No valid attribute type found is less than 2 or more than 7 characters in length.", nameof(prefix));
			return false;
		}
		else if (prefix.ContainsAnyExcept(UniqueAttributeChars))
		{
			exception = new ArgumentException($"The specified attribute type contains invalid characters: {prefix.ToString()}", nameof(prefix));
			return false;
		}

		exception = null;
		return true;
	}
	private static bool TryGetRelativeNameType(ReadOnlySpan<char> prefix, out RelativeNameType result)
	{
		if (prefix.Length >= 2 && prefix.Length <= 7 && !prefix.ContainsAnyExcept(UniqueAttributeChars))
		{
			foreach (var kvp in _attributeValues)
			{
				if (prefix.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
				{
					result = kvp.Value;
					return true;
				}
			}
		}

		result = default;
		return false;
	}

	public static bool TryParse(ReadOnlySpan<char> value, RelativeNameType typeToUseWhenNotPresent, out RelativeName relativeName)
	{
		if (!IsValid(value, out int equalsIndex))
		{
			relativeName = Empty;
			return false;
		}

		if (equalsIndex < MINIMUM_NAME_INDEX)
		{
			if (typeToUseWhenNotPresent == RelativeNameType.None || !AttributeStrings.TryGetValue(typeToUseWhenNotPresent, out string? prefix))
			{
				relativeName = Empty;
				return false;
			}

			int prefixLength = prefix.Length;
			Span<char> chars = stackalloc char[value.Length + prefixLength];
			prefix.CopyTo(chars);
			value.CopyTo(chars.Slice(prefixLength));

			relativeName = new(typeToUseWhenNotPresent, new string(chars), in prefixLength);
			return true;
		}
		else if (TryGetRelativeNameType(value.Slice(0, equalsIndex + 1), out var type)
				 &&
				 AttributeStrings.TryGetValue(type, out string? prefix))
		{
			if (!value.Slice(0, equalsIndex + 1).Equals(prefix, StringComparison.Ordinal))
			{
				int prefixLength = prefix.Length;
				Span<char> chars = stackalloc char[value.Length - equalsIndex + prefixLength];
				prefix.CopyTo(chars);
				value.Slice(equalsIndex + 1).CopyTo(chars.Slice(prefixLength));

				relativeName = new(type, new string(chars), in prefixLength);
			}
			else
			{
				relativeName = new(type, value.ToString(), prefix.Length);
			}

			return true;
		}
		else
		{
			relativeName = Empty;
			return false;
		}
	}

	/// <summary>
	/// Attempts to determine the relative name type from the specified span of characters.
	/// </summary>
	/// <param name="value">The read-only span of characters to check.</param>
	/// <param name="result">
	/// When this method returns, contains the relative name type if <paramref name="value"/> is prefaced with a valid
	/// attribute value. This parameter is passed uninitialized.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if <paramref name="value"/> is prefaced with a valid attribute value;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public static bool TryReadRelativeNameType(ReadOnlySpan<char> value, out RelativeNameType result)
	{
		if (value.IsWhiteSpace() || !TryGetRelativeNameType(value, out result))
		{
			result = default;
			return false;
		}

		return true;
	}

	/// <summary>
	/// Attempts to parse a single relative distinguished name from the specified span of characters.
	/// </summary>
	/// <param name="value">The read-only span of characters to parse.</param>
	/// <param name="result">
	/// When this method returns, contains the parsed <see cref="RelativeName"/> if successful.
	/// This parameter is passed uninitialized.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the span was successfully parsed into a <see cref="RelativeName"/>;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public static bool TryParseOne(ReadOnlySpan<char> value, out RelativeName result)
	{
		scoped ReadOnlySpan<char> span = value;
		if (!IsValid(value))
		{
			result = Empty;
			return false;
		}

		char[]? array = null;
		bool isRented = false;

		int index = span.IndexOf('=') + 1;
		scoped ReadOnlySpan<char> prefix;
		if (index >= span.Length)
		{
			result = Empty;
			return false;
		}

		if (span.IsEmpty)
		{
			result = Empty;
			return false;
		}

		if (index < MINIMUM_NAME_INDEX)
		{
			result = Empty;
			return false;
		}

		prefix = span.Slice(0, index);
		if (!TryGetRelativeNameType(prefix, out var type))
		{
			result = Empty;
			return false;
		}

		result = new(type, span.ToString(), index);
		if (isRented)
		{
			ArrayPool<char>.Shared.Return(array!);
		}

		return true;
	}

	/// <summary>
	/// Returns the attributed string representation of the relative name.
	/// </summary>
	/// <returns>
	/// The attributed string representation of the relative name in the format with no separating commas:
	/// <c>AttributeType=Name</c>.
	/// </returns>
	public override string ToString()
	{
		return this.Value;
	}

	/// <summary>
	/// Throws an <see cref="ArgumentException"/> if the specified prefix is not valid.
	/// </summary>
	/// <param name="prefix">The read-only span of characters to validate as a prefix.</param>
	/// <exception cref="ArgumentException">
	/// Thrown when the specified prefix is less than 2 or more than 7 characters in length,
	/// or contains invalid characters.
	/// </exception>
	public static void ThrowWhenInvalidPrefix(ReadOnlySpan<char> prefix)
	{
		if (prefix.IsEmpty)
		{
			return;
		}

		if (!IsValidPrefix(prefix, out ArgumentException? exception))
		{
			throw exception;
		}
	}

	/// <summary>
	/// Registers the dependencies for the <see cref="RelativeName"/> struct.
	/// </summary>
	/// <param name="services">The service collection to add the dependencies to.</param>
	[DynamicDependencyRegistrationMethod]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private static void AddToServices(IServiceCollection services)
	{
		services.AddSingleton(AttributeStrings)
				.AddSingleton(AttributeStrings.EnumStrings);
	}
}
