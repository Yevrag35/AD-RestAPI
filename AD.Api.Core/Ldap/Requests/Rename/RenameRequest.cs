using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Ldap.Requests
{
    public class RenameRequest : IValidatableObject
    {
        [Required]
        [MinLength(1, ErrorMessage = "Directory names must be at least 1 character in length.")]
        public required string Name { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return [];
        }
    }
}
