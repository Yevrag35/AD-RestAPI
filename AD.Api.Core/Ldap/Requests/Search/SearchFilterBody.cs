using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Requests;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using AD.Api.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap;

public sealed class SearchFilterBody : IScopedRequest, ISearchFilter, IValidatableObject
{
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

	[DistinguishedName(AllowEmpty = true)]
	public DistinguishedName? SearchBase { get; set; }

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
	public DistinguishedName GetScopedPath()
	{
		return this.SearchBase ?? DistinguishedName.Empty;
	}
	string IScopedRequest.GetScopedPathMemberName()
	{
		return nameof(this.SearchBase);
	}
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		ReadOnlySpan<char> filter = this.Filter;
		if (!IsAllFilter(filter) && !filter.ContainsEqualAmount('(', ')'))
		{
			yield return new ValidationResult("The LDAP filter is not properly formatted - are you missing parentheses?", [nameof(this.Filter)]);
		}
	}
}

