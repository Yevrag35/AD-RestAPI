using AD.Api.Core.Ldap;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field,
        AllowMultiple = false, Inherited = true)]
    public sealed class DistinguishedNameAttribute : ValidationAttribute
    {
        public bool AllowEmpty { get; init; }
        public bool RequireParent { get; init; }
        public RelativeNameType RequiredRelativeNameType { get; init; } = RelativeNameType.None;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is not DistinguishedName dn || (dn.IsEmpty && !this.AllowEmpty))
            {
                return new ValidationResult("The distinguished name must not be empty.", GetMemberName(validationContext));
            }

            if (this.RequireParent && !dn.HasParent)
            {
                return new ValidationResult("The distinguished name must have a parent path.", GetMemberName(validationContext));
            }

            if (this.RequiredRelativeNameType != RelativeNameType.None
                &&
                dn.Type != this.RequiredRelativeNameType)
            {
                return new ValidationResult($"The distinguished name must be of type {this.RequiredRelativeNameType}.", GetMemberName(validationContext));
            }

            return ValidationResult.Success;
        }

        private static IEnumerable<string> GetMemberName(ValidationContext context)
        {
            yield return context.MemberName ?? string.Empty;
        }
    }
}
