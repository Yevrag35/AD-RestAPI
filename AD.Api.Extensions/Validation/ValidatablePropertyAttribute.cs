using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace AD.Api.Validation
{
    public abstract class ValidatablePropertyAttribute<T> : ValidationAttribute
    {
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
}

