using AD.Api.Core.Ldap.Filters;
using AD.Api.Statics;
using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
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
        [MinLength(1, ErrorMessage = "Filter should be an LDAP-formatted string or the '*' character for all.")]
        public required string Filter
        {
            get => _filter ??= string.Empty;
            set
            {
                _filter = value;
                this.HasLdapFilter = !string.IsNullOrWhiteSpace(value);
            }
        }

        public string[] Properties { get; init; } = [];

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public FilteredRequestType? RequestBaseType { get; set; }

        public SearchScope? Scope { get; init; }

        [MinLength(4, ErrorMessage = "The distinguishedName of the SearchBase is not formatted correctly.")]
        public string? SearchBase { get; set; }

        [Range(0, int.MaxValue)]
        public int? SizeLimit { get; init; }

        [MinLength(1, ErrorMessage = "Sort properties must be at least 1 character in length.")]
        public string? SortBy { get; init; }

        [AllowedValues("asc", "desc", "0", "1", "ascending", "descending", "", null)]
        public string? SortDirection { get; init; }

        private static bool IsAllFilter(ReadOnlySpan<char> filter)
        {
            return filter.Length == 1 && CharConstants.STAR == filter[0];
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            ReadOnlySpan<char> filter = this.Filter;
            if (!IsAllFilter(filter) && filter.Count('(') != filter.Count(')'))
            {
                yield return new ValidationResult("The LDAP filter is not properly formatted - are you missing parentheses?", [nameof(this.Filter)]);
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

