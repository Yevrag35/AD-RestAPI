using AD.Api.Core.Ldap;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field,
	AllowMultiple = false, Inherited = true)]
public sealed class DistinguishedNameAttribute : ValidationAttribute
{
	public bool AllowEmpty { get; init; }
	public int MinimumSegmentCount { get; init; }
	public bool RequireParent { get; init; }
	public RelativeNameType RequiredRelativeNameType { get; init; } = RelativeNameType.None;

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		ValidationResult? result = ValidationResult.Success;
		if (value is null)
			return result;

		if (this.TryIsNotDNOrEmptyNotAllowed(value, validationContext, out DistinguishedName dn, ref result))
			return result;

		if (this.TryIsBelowMinimumSegmentCount(in dn, validationContext, ref result))
			return result;

		if (this.TryIsInvalidParent(in dn, validationContext, ref result))
			return result;

		_ = this.TryIsInvalidRelativeNameType(in dn, validationContext, ref result);
		return result;
	}

	private static IEnumerable<string> GetMemberName(ValidationContext context)
	{
		return [context.MemberName ?? context.DisplayName ?? AttributeConstants.DISTINGUISHED_NAME];
	}
	private bool IsEmptyAndNotAllowed(in DistinguishedName dn)
	{
		return !this.AllowEmpty && dn.IsEmpty;
	}

	private bool TryIsBelowMinimumSegmentCount(in DistinguishedName dn, ValidationContext context, [NotNullWhen(true)] ref ValidationResult? result)
	{
		if (this.MinimumSegmentCount > 0 && dn.Count < this.MinimumSegmentCount)
		{
			result = new ValidationResult($"The distinguishedName must have a minimum of {this.MinimumSegmentCount} segments.", GetMemberName(context));
			return true;
		}

		return false;
	}
	private bool TryIsInvalidParent(in DistinguishedName dn, ValidationContext context, [NotNullWhen(true)] ref ValidationResult? result)
	{
		if (this.RequireParent && !this.AllowEmpty && !dn.HasParent)
		{
			result = new ValidationResult("The distinguished name must have a parent path.", GetMemberName(context));
			return true;
		}

		return false;
	}
	private bool TryIsInvalidRelativeNameType(in DistinguishedName dn, ValidationContext context, [NotNullWhen(true)] ref ValidationResult? result)
	{
		if (this.RequiredRelativeNameType != RelativeNameType.None
			&&
			dn.Type != this.RequiredRelativeNameType)
		{
			result = new ValidationResult($"The distinguished name must be of type {this.RequiredRelativeNameType}.", GetMemberName(context));
			return true;
		}

		return false;
	}
	private bool TryIsNotDNOrEmptyNotAllowed(object value, ValidationContext context, out DistinguishedName distinguishedName, [NotNullWhen(true)] ref ValidationResult? result)
	{
		if (value is not DistinguishedName dn || this.IsEmptyAndNotAllowed(in dn))
		{
			distinguishedName = DistinguishedName.Empty;
			result = new ValidationResult("The distinguished name must not be empty.", GetMemberName(context));
			return true;
		}

		distinguishedName = dn;
		return false;
	}
}
