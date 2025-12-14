using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation;

public static class ValidationResultFactory
{
	public static ValidationResult? IsStringNullEmptyOrWhitespace(string? value, [CallerArgumentExpression(nameof(value))] string paramName = "")
	{
		return string.IsNullOrWhiteSpace(value)
			? new ValidationResult(Errors.Validation_NullEmptyOrWS, [paramName])
			: ValidationResult.Success;
	}
	public static ValidationResult? IsStringNullEmptyOrWhitespace<T>(string? value, T state, Func<T, IEnumerable<string>> memberNamesFunc)
	{
		return string.IsNullOrWhiteSpace(value)
			? new ValidationResult(Errors.Validation_NullEmptyOrWS, memberNamesFunc(state))
			: ValidationResult.Success;
	}
}