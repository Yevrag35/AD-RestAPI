using AD.Api.Buffers;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation;

/// <summary>
/// Specifies that a property must contain a valid SHA-1 thumbprint string for validation purposes.
/// </summary>
/// <remarks>A valid SHA-1 thumbprint is a 40-character hexadecimal string consisting only of the characters 0-9,
/// a-f, or A-F. This attribute can be applied to string properties to enforce this format during validation. If the
/// property implements <see cref="IValidatableThumbprint"/>, validation is delegated to the nested property.</remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class SHA1ThumbprintAttribute : ValidatablePropertyAttribute<string>
{
	private const int THUMBPRINT_LENGTH = 40;
	private static readonly SearchValues<char> s_sha1Chars;
	static SHA1ThumbprintAttribute()
	{
		CharRange numerals = new('0', '9');
		CharRange lowercase = new('a', 'f');
		CharRange uppercase = new('A', 'F');

		Span<char> chars = stackalloc char[numerals.Length + lowercase.Length + uppercase.Length];
		numerals.CopyTo(chars);
		int pos = numerals.Length;

		lowercase.CopyTo(chars.Slice(pos));
		pos += lowercase.Length;

		uppercase.CopyTo(chars.Slice(pos));

		s_sha1Chars = SearchValues.Create(chars);
	}

	public SHA1ThumbprintAttribute()
	{
		this.ErrorMessageResourceName = nameof(Errors.Validation_Thumbprint_NotString);
		this.ErrorMessageResourceType = typeof(Errors);
	}

	public override bool RequiresValidationContext => false;
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is null)
		{
			return ValidationResult.Success;
		}
		else if (value is IValidatableThumbprint validatable)
		{
			return ValidateNestedProperty(validatable, validationContext);
		}

		string? msg = null;
		if (value is not string sha1Str)
		{
			msg = this.FormatErrorMessage(value.ToString() ?? string.Empty);
		}
		else if (sha1Str.Length != THUMBPRINT_LENGTH)
		{
			msg = string.Format(Errors.Validation_Thumbprint_IncorrectLength, sha1Str.Length);
		}
		else if (sha1Str.AsSpan().ContainsAnyExcept(s_sha1Chars))
		{
			msg = string.Format(Errors.Validation_Thumbprint_InvalidChars, sha1Str);
		}

		return msg is not null
			? new ValidationResult(msg, validationContext.GetMemberNames())
			: ValidationResult.Success;
	}
}

