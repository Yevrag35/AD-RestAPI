using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap
{
    public sealed class SearchFilterBody : IValidatableObject
    {
        [Required]
        [MinLength(2)]
        [JsonPropertyName("filter")]
        public required string LdapFilter { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            ReadOnlySpan<char> filter = this.LdapFilter;
            if (filter.Count('(') != filter.Count(')'))
            {
                yield return new ValidationResult("The LDAP filter is not properly formatted - are you missing parentheses?", [nameof(this.LdapFilter)]);
            }
        }
    }
}

