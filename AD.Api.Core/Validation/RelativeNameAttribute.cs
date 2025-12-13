using AD.Api.Core.Ldap;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field,
	AllowMultiple = false, Inherited = true)]
public sealed class RelativeNameAttribute : ValidationAttribute
{
	private const string COMMON_NAME = "CommonName (cn)";

	public bool AllowEmpty { get; init; }
	public RelativeNameType RequiredType { get; init; } = RelativeNameType.None;

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		ValidationResult? result = ValidationResult.Success;
		if (value is null)
			return result;

		if (this.IsNotRDNOrEmptyNotAllowed(value, validationContext, out RelativeName rdn, ref result))
			return result;

		_ = this.IsNotRequiredType(in rdn, validationContext, ref result);
		return result;
	}

	private bool IsEmptyAndNotAllowed(in RelativeName rdn)
	{
		return !this.AllowEmpty && rdn.IsEmpty;
	}
	private bool IsNotRDNOrEmptyNotAllowed(object value, ValidationContext context, out RelativeName rdn, [NotNullWhen(true)] ref ValidationResult? result)
	{
		if (value is not RelativeName relativeName || this.IsEmptyAndNotAllowed(in relativeName))
		{
			rdn = RelativeName.Empty;
			result = new ValidationResult("The relative name must not be null, empty, or whitespace.", context.GetMemberNames());
			return true;
		}

		rdn = relativeName;
		return false;
	}
	private bool IsNotRequiredType(in RelativeName rdn, ValidationContext context, [NotNullWhen(true)] ref ValidationResult? result)
	{
		if (this.RequiredType != RelativeNameType.None && rdn.AttributeType != this.RequiredType)
		{
			result = new ValidationResult($"The relative name must be of type {this.RequiredType}.", context.GetMemberNames());
			return true;
		}

		return false;
	}
}