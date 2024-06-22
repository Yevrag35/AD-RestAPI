using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap
{
    public sealed class SearchFilterBody : ISearchFilter, IValidatableObject
    {
        private static readonly SearchValues<char> _mustContain = SearchValues.Create([',', '=']);
        private const string DN = ",DC=";

        private string? _filter;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool HasLdapFilter { get; private set; }

        [Required]
        [MinLength(2)]
        [JsonPropertyName("filter")]
        public required string LdapFilter
        {
            get => _filter ??= string.Empty;
            set
            {
                _filter = value;
                this.HasLdapFilter = !string.IsNullOrWhiteSpace(value);
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public FilteredRequestType? RequestBaseType { get; set; }

        [MinLength(4, ErrorMessage = "The distinguishedName of the SearchBase is not formatted correctly.")]
        public string? SearchBase { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            ReadOnlySpan<char> filter = this.LdapFilter;
            if (filter.Count('(') != filter.Count(')'))
            {
                yield return new ValidationResult("The LDAP filter is not properly formatted - are you missing parentheses?", [nameof(this.LdapFilter)]);
            }

            ReadOnlySpan<char> sb = this.SearchBase;

            if (!sb.IsEmpty && (!sb.ContainsAnyExcept(_mustContain) || !sb.Contains(DN, StringComparison.OrdinalIgnoreCase)))
            {
                yield return new("The distinguishedName of the SearchBase is not formatted correctly.",
                    [nameof(this.SearchBase)]);
            }
        }
    }
}

