using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Validation;

/// <summary>
/// Specifies that a property or parameter must be a valid URI, optionally restricting the URI kind to absolute or
/// relative.
/// </summary>
/// <remarks>Apply this attribute to properties or parameters to enforce URI validation during model binding or
/// data validation. By setting the Kind property, you can require that the value be an absolute or relative URI. The
/// attribute supports validation of both string and Uri types. If the value is a string, it is validated using the
/// specified UriKind. If the value is null, validation succeeds. This attribute is typically used in ASP.NET Core or
/// other frameworks that support data annotations.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class IsUriAttribute : ValidationAttribute
{
	/// <summary>
	/// Gets the required kind of URI the value must be.
	/// </summary>
	public UriKind Kind { get; init; }

	/// <summary>
	/// Initializes a new instance of the <see cref="IsUriAttribute"/> class, which validates that a field or property
	/// contains a valid URI value.
	/// </summary>
	public IsUriAttribute()
		: base("The {0} field is not a valid URI.")
	{
	}

	/// <inheritdoc/>
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		return value switch
		{
			Uri uri when (uri.IsAbsoluteUri && this.Kind == UriKind.Relative)
					   || (!uri.IsAbsoluteUri && this.Kind == UriKind.Absolute) => Fail(this, validationContext),
			string str when !Uri.IsWellFormedUriString(str, this.Kind) => Fail(this, validationContext),
			_ => ValidationResult.Success,
		};

		static ValidationResult Fail(IsUriAttribute attribute, ValidationContext context)
		{
			return new ValidationResult(attribute.FormatErrorMessage(context.DisplayName));
		}
	}
}
