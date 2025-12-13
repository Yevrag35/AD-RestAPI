using System.Collections;
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AD.Api.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ValidateEncodingAttribute : ValidatablePropertyAttribute<string>
{
	private static readonly FrozenSet<string> _encodingNames;
	static ValidateEncodingAttribute()
	{
		_encodingNames = Encoding.GetEncodings()
								 .Select(x => x.Name)
								 .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
	}

	private readonly IReadOnlySet<string> _validEncodingNames;

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidateEncodingAttribute"/> class with optionally specified
	/// valid encoding names where at least 1 must be matched.
	/// </summary>
	/// <param name="validEncodings"></param>
	/// <exception cref="ArgumentException"></exception>
	public ValidateEncodingAttribute(params string[] validEncodings)
	{
		if (validEncodings is null || validEncodings.Length == 0)
		{
			_validEncodingNames = _encodingNames;
			return;
		}

		HashSet<string> set = new(validEncodings, StringComparer.OrdinalIgnoreCase);
		if (!_encodingNames.IsSupersetOf(set))
		{
			set.ExceptWith(_encodingNames);
			throw new ArgumentException($"The following encoding names are not valid: {string.Join(", ", set)}", nameof(validEncodings));
		}

		_validEncodingNames = set;
	}

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is null)
		{
			return ValidationResult.Success;
		}
		else if (value is IValidatableEncoding validatable)
		{
			return ValidateNestedProperty(validatable, validationContext);
		}

		HashSet<string> encodingNames;

		if (value is Encoding encoding)
		{
			encodingNames = [encoding.EncodingName];
		}
		else if (value is string encStr)
		{
			encodingNames = [encStr];
		}
		else if (value is IEnumerable<string> stringEnumerable)
		{
			encodingNames = [.. stringEnumerable];
		}
		else if (value is IEnumerable enumerable)
		{
			try
			{
				encodingNames = [.. enumerable.Cast<string>()];
			}
			catch (InvalidCastException)
			{
				return new ValidationResult("The value must be a string or System.Text.Encoding object.", validationContext.GetMemberNames());
			}
		}
		else
		{
			return new ValidationResult("The value must be a string or System.Text.Encoding object.");
		}

		if (!_validEncodingNames.IsSupersetOf(encodingNames))
		{
			encodingNames.ExceptWith(_validEncodingNames);
			return new ValidationResult($"The following encoding names are not valid: {string.Join(", ", encodingNames)}", validationContext.GetMemberNames());
		}

		return ValidationResult.Success;
	}
}

