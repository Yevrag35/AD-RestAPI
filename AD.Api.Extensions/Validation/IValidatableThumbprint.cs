namespace AD.Api.Validation;

/// <summary>
/// Represents a string-based thumbprint value that can be validated according to custom rules.
/// </summary>
/// <remarks>Implementations define the validation logic for the thumbprint value. This interface is typically
/// used to ensure that thumbprint values meet specific format or integrity requirements before use in security or
/// identification scenarios.</remarks>
public interface IValidatableThumbprint : IValidatableProperty<string>
{
}

