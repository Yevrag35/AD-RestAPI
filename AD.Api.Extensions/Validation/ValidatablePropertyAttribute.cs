using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace AD.Api.Validation;

/// <summary>
/// Specifies an abstract base attribute for validating properties of type T using custom validation logic.
/// </summary>
/// <remarks>Inherit from this attribute to implement custom validation for properties of a specific type. This
/// attribute is intended for use on properties that require validation beyond standard data annotation attributes.
/// Derived classes should override the validation logic as needed.</remarks>
/// <typeparam name="T">The type of the property to be validated.</typeparam>
public abstract class ValidatablePropertyAttribute<T> : ValidationAttribute
{
	/// <summary>
	/// Validates a nested property of the specified validatable property using its associated validation attribute.
	/// </summary>
	/// <remarks>This method inspects the nested property for a <see cref="ValidatablePropertyAttribute{T}"/> and, if present,
	/// invokes its validation logic. If the property or attribute is not found, or if the property value is null, the
	/// method returns <see cref="ValidationResult.Success"/>.</remarks>
	/// <param name="value">The validatable property instance containing the nested property to validate. Cannot be null.</param>
	/// <param name="context">The validation context that provides information about the validation operation.</param>
	/// <returns>A <see cref="ValidationResult"/> that indicates the result of the validation. Returns <see cref="ValidationResult.Success"/> if the property is
	/// valid or not applicable; otherwise, returns a <see cref="ValidationResult"/> describing the validation failure.</returns>
	protected static ValidationResult? ValidateNestedProperty(IValidatableProperty<T> value, ValidationContext context)
	{
		var expression = value.GetValidatableProperty();
		if (expression is null || expression.Body is not MemberExpression memEx || memEx.Member is not PropertyInfo propInfo)
		{
			return ValidationResult.Success;
		}

		ValidatablePropertyAttribute<T>? propAtt = propInfo.GetCustomAttribute<ValidatablePropertyAttribute<T>>();
		if (propAtt is null)
		{
			return ValidationResult.Success;
		}

		var func = expression.Compile();
		object? propValue = func(value);

		if (propValue is null)
		{
			return ValidationResult.Success;
		}

		ValidationContext newContext = new(value, context, context.Items)
		{
			MemberName = propInfo.Name,
			DisplayName = propInfo.Name,
		};

		return propAtt.GetValidationResult(propValue, newContext);
	}
}

