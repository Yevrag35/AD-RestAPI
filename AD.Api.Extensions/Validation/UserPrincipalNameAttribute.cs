using AD.Api.Buffers;
using AD.Api.Statics;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
	AllowMultiple = false, Inherited = true)]
public sealed class UserPrincipalNameAttribute : ValidationAttribute
{
	const string UPN = "UserPrincipalName";
	const int SYMBOL_COUNT = 9;
	static readonly SearchValues<char> _allAllowedChars;
	static readonly SearchValues<char> _allowedMinusAt;
	static readonly char AT_SIGN = CharConstants.AT_SIGN;

	static UserPrincipalNameAttribute()
	{
		CharRange upper = new('A', 'Z');
		CharRange lower = new('a', 'z');
		CharRange digit = new('0', '9');

		int rangeLength = upper.Length + lower.Length + digit.Length;
		Span<char> allowed = stackalloc char[rangeLength + SYMBOL_COUNT];
		int index = 0;
		allowed[index++] = CharConstants.AT_SIGN;
		upper.CopyTo(allowed[index..]);
		index += upper.Length;
		lower.CopyTo(allowed[index..]);
		index += lower.Length;
		digit.CopyTo(allowed[index..]);
		index += digit.Length;

		AddAllowedSymbols(allowed, index);
		_allAllowedChars = SearchValues.Create(allowed);
		_allowedMinusAt = SearchValues.Create(allowed[1..]);
	}
	private static void AddAllowedSymbols(Span<char> chars, int index)
	{
		chars[index++] = CharConstants.SINGLE_QUOTE;
		chars[index++] = CharConstants.PERIOD;
		chars[index++] = CharConstants.HYPHEN;
		chars[index++] = CharConstants.UNDERSCORE;
		chars[index++] = CharConstants.BANG;
		chars[index++] = CharConstants.POUND;
		chars[index++] = '~';
		chars[index++] = '^';
	}

	/// <summary>
	/// Indicates whether to treat an empty string as valid.
	/// </summary>
	public bool AllowEmpty { get; init; }

	public UserPrincipalNameAttribute()
	{
		this.ErrorMessageResourceType = typeof(Errors);
		this.ErrorMessageResourceName = nameof(Errors.Validation_InvalidUPN_Format);
	}

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is null)
		{
			return ValidationResult.Success;
		}

		if (value is not string strValue || IsEmptyAndNotAllowed(strValue, this.AllowEmpty) || strValue.Length <= 2)
		{
			return new ValidationResult(this.ErrorMessageString, validationContext.GetMemberNames());
		}

		ReadOnlySpan<char> chars = strValue.AsSpan();

		if (chars.ContainsAnyExcept(_allAllowedChars) || !chars.ContainsAny(_allowedMinusAt))
		{
			return new ValidationResult(Errors.Validation_InvalidUPN_Format, validationContext.GetMemberNames());
		}

		int index = chars.IndexOf(CharConstants.AT_SIGN);
		if (index <= 0 || index >= chars.Length - 1)
		{
			return new ValidationResult(Errors.Validation_InvalidUPN_Format, validationContext.GetMemberNames());
		}

		int count = 0;
		foreach (Range section in chars.Split(AT_SIGN))
		{
			count++;
			if (count > 2)
			{
				return new ValidationResult(Errors.Validation_InvalidUPN_Format, validationContext.GetMemberNames());
			}

			if (chars[section].IsWhiteSpace())
			{
				count--;
				continue;
			}
		}

		if (count <= 1)
		{
			return new ValidationResult(Errors.Validation_InvalidUPN_Format, validationContext.GetMemberNames());
		}

		return ValidationResult.Success;
	}

	private static bool IsEmptyAndNotAllowed(ReadOnlySpan<char> chars, bool allowEmpty)
	{
		return chars.IsEmpty && !allowEmpty;
	}
}
