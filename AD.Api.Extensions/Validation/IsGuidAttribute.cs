using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation;

/// <summary>
/// Validation attribute that checks whether a string value is a valid, non-empty <see cref="Guid"/>.
/// </summary>
/// <remarks>
/// Use this attribute to ensure that a property contains a well-formed and non-default GUID string.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class IsGuidAttribute : ValidationAttribute
{
	/// <summary>
	/// Gets a value indicating whether empty strings are considered valid input.
	/// </summary>
	public bool AllowEmptyString { get; init; }
	/// <summary>
	/// Gets a value indicating whether environment variables are permitted for configuration or substitution.
	/// </summary>
	/// <remarks>Set this property to <see langword="true"/> to allow the use of environment variables in
	/// configuration sources or values. This can enable dynamic configuration based on the runtime environment. If <see
	/// langword="false"/>, environment variables will be ignored or unavailable for configuration purposes.</remarks>
	public bool AllowEnvironmentVariable { get; init; }

	/// <summary>
	/// Initializes a new instance of the <see cref="IsGuidAttribute"/> class.
	/// </summary>
	public IsGuidAttribute() { }

	/// <summary>
	/// Determines whether the specified value is a valid, non-empty <see cref="Guid"/> string.
	/// </summary>
	/// <param name="value">The value of the object to validate. Expected to be a <see langword="string"/>.</param>
	/// <param name="validationContext">The context information about the validation operation.</param>
	/// <returns>
	/// <see langword="null"/> if the value is <see langword="null"/> or a valid, non-empty GUID string; 
	/// otherwise, a <see cref="ValidationResult"/> describing the validation failure.
	/// </returns>
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is null || value is not string strValue)
			return ValidationResult.Success;

		if ((this.AllowEmptyString && strValue.Length == 0) || (this.AllowEnvironmentVariable && strValue.StartsWith("{env:", StringComparison.OrdinalIgnoreCase) && strValue[^1] == '}'))
			return ValidationResult.Success;

		return Guid.TryParse(strValue, out Guid result) && Guid.Empty != result
			? ValidationResult.Success
			: new ValidationResult(
				errorMessage: "Not a valid, populated (non-empty) GUID",
				memberNames: [validationContext.MemberName ?? validationContext.DisplayName]);
	}
}