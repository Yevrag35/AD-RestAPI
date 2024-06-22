using AD.Api.Pooling;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
using SR = System.DirectoryServices.Protocols.SearchRequest;

namespace AD.Api.Core.Ldap
{
    public class SearchParameters : IValidatableObject
    {
        private static readonly FrozenSet<string> _descOrder =
            FrozenSet.ToFrozenSet(["desc", "descending", "1"], StringComparer.OrdinalIgnoreCase);

        private string? _domain;

        [FromQuery]
        [Base64String]
        public string? Cookie { get; set; }

        [FromQuery(Name = "domain")]
        public string? Domain
        {
            get => _domain;
            set => _domain = value;
        }

        [FromServices]
        public required ILdapFilterService FilterSvc { get; init; }

        [FromQuery(Name = "scope")]
        public SearchScope? Scope { get; set; }

        [FromQuery]
        [Range(0, int.MaxValue)]
        public int? PageSize { get; set; }

        [FromQuery(Name = "properties")]
        public string? Properties { get; set; }

        [FromQuery(Name = "limit")]
        [Range(0, int.MaxValue)]
        public int? SizeLimit { get; set; }

        [FromQuery(Name = "sortDir")]
        [AllowedValues("asc", "desc", "0", "1", "ascending", "descending", "", null)]
        public string? SortDirection { get; set; }

        [FromQuery(Name = "sortBy")]
        public string? SortProperty { get; set; }

        [FromServices]
        public required IPooledItem<LdapSearchRequest> SearchRequest { get; init; }

        [MemberNotNull(nameof(Domain))]
        public LdapConnection ApplyConnection(IConnectionService connectionService)
        {
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
            return this.SearchRequest.Value.ApplyConnection(ref _domain, connectionService);
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
        }

        public void ApplyParameters(ISearchFilter searchFilter)
        {
            this.SearchRequest.Value.AddAttributes(this.Properties, searchFilter.RequestBaseType);

            SR request = this.SearchRequest.Value.AsLdapRequest();
            if (this.Scope.HasValue)
            {
                request.Scope = this.Scope.Value;
            }

            if (this.SizeLimit.HasValue)
            {
                this.SearchRequest.Value.SizeLimit = this.SizeLimit.Value;
            }

            this.SearchRequest.Value.SetCookie(this.PageSize, this.Cookie);

            if (searchFilter.HasLdapFilter)
            {
                request.Filter = searchFilter.LdapFilter;
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.SearchBase))
            {
                request.DistinguishedName = searchFilter.SearchBase;
            }

            if (!string.IsNullOrWhiteSpace(this.SortProperty))
            {
                this.SortDirection ??= string.Empty;
                SortRequestControl control = new(this.SortProperty, _descOrder.Contains(this.SortDirection));
                request.Controls.Add(control);
            }
        }

        public static implicit operator SR(SearchParameters parameters)
        {
            return parameters.SearchRequest.Value.AsLdapRequest();
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IConnectionService conSvc = validationContext.GetRequiredService<IConnectionService>();
            if (!conSvc.RegisteredConnections.ContainsKey(this.Domain))
            {
                yield return new ValidationResult($"'{this.Domain}' - Domain not found", [nameof(this.Domain)]);
            }
        }
    }
}

