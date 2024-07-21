using AD.Api.Attributes.Services;
using AD.Api.Serialization.Json;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field,
    AllowMultiple = false, Inherited = true)]
public sealed class RequiredAfterDeserializationAttribute : ValidationAttribute
{
    public const string RequiredAfterKey = "Required";

    public bool AllowEmpty { get; init; }

    public RequiredAfterDeserializationAttribute()
    {
        this.ErrorMessageResourceName = nameof(Errors.Validation_EmptyObj_NotAllowed);
        this.ErrorMessageResourceType = typeof(Errors);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is IJsonDeserializableOnce deserializable && !deserializable.IsDeserialized && !string.IsNullOrWhiteSpace(validationContext.MemberName))
        {
            deserializable.OnDeserialized();
            value = GetMemberValue(validationContext, validationContext.MemberName);
        }

        if (value is null)
        {
            return new ValidationResult(this.ErrorMessageString, validationContext.GetMemberNames());
        }

        bool isEmpty = false;
        if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
        {
            isEmpty = true;
        }
        else if (value is ICanBeEmpty canBe && canBe.IsEmpty)
        {
            isEmpty = true;
        }

        return isEmpty && !this.AllowEmpty
            ? new ValidationResult(this.ErrorMessageString, validationContext.GetMemberNames())
            : ValidationResult.Success;
    }

    private static object? GetMemberValue(ValidationContext context, string memberName)
    {
        return context.ObjectType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(context.ObjectInstance);
    }
}