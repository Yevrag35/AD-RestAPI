using AD.Api.Core.Ldap.Filters;
using AD.Api.Pooling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
using SR = System.DirectoryServices.Protocols.SearchRequest;

namespace AD.Api.Core.Ldap
{
    public class SearchParameters : RequestParameters<LdapSearchRequest, SearchResponse>, IValidatableObject
    {
        private static readonly FrozenSet<string> _descOrder =
            FrozenSet.ToFrozenSet(["desc", "descending", "1"], StringComparer.OrdinalIgnoreCase);

        internal ISearchFilter? BackingFilter { get; set; }

        [FromServices]
        public required ILdapFilterService FilterSvc { get; init; }

        [FromQuery(Name = "scope")]
        public SearchScope? Scope { get; set; }

        //[FromQuery]
        //[Range(0, int.MaxValue)]
        //public int? PageSize { get; set; }

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
        public required IPooledItem<LdapSearchRequest> SearchRequest { get; set; }
        [BindNever]
        public override LdapSearchRequest Request => this.SearchRequest.Value;

        public virtual void ApplyParameters(ISearchFilter? searchFilter)
        {
            if (searchFilter is null)
            {
                if (this.BackingFilter is null)
                {
                    throw new ArgumentNullException(nameof(searchFilter));
                }

                searchFilter = this.BackingFilter;
            }
            else
            {
                this.BackingFilter = searchFilter;
            }

            if (searchFilter.Properties is not null)
            {
                this.SearchRequest.Value.AddAttributes(searchFilter.Properties, searchFilter.RequestBaseType);
            }
            else
            {
                this.SearchRequest.Value.AddAttributes(this.Properties, searchFilter.RequestBaseType);
            }

            SR request = this.SearchRequest.Value.AsLdapRequest();
            if (searchFilter.Scope.HasValue)
            {
                this.Scope = searchFilter.Scope;
                request.Scope = searchFilter.Scope.Value;
            }
            else if (this.Scope.HasValue)
            {
                request.Scope = this.Scope.Value;
            }

            //if (this.PageSize.HasValue)
            //{
            //    this.SearchRequest.Value.PageSize = this.PageSize.Value;
            //}

            if (searchFilter.SizeLimit.HasValue)
            {
                this.SizeLimit = searchFilter.SizeLimit;
                this.SearchRequest.Value.SizeLimit = searchFilter.SizeLimit.Value;
            }
            else if (this.SizeLimit.HasValue)
            {
                this.SearchRequest.Value.SizeLimit = this.SizeLimit.Value;
            }

            if (searchFilter.HasLdapFilter)
            {
                request.Filter = searchFilter.Filter;
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.SearchBase))
            {
                request.DistinguishedName = searchFilter.SearchBase;
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.SortBy))
            {
                this.SortProperty = searchFilter.SortBy;
                this.SortDirection ??= searchFilter.SortDirection ?? string.Empty;
                SortRequestControl control = new(searchFilter.SortBy, _descOrder.Contains(this.SortDirection));
                request.Controls.Add(control);
            }
            else if (!string.IsNullOrWhiteSpace(this.SortProperty))
            {
                this.SortDirection ??= string.Empty;
                SortRequestControl control = new(this.SortProperty, _descOrder.Contains(this.SortDirection));
                request.Controls.Add(control);
            }
        }

        public void Dissipate()
        {
            this.SearchRequest = null!;
        }

        protected override void OnApplyingConnection(ConnectionContext context)
        {
            this.SearchRequest.Value.ApplyContext(context);
        }
        public void Rehydrate(HttpContext context)
        {
            this.SearchRequest = context.RequestServices.GetRequiredService<IPooledItem<LdapSearchRequest>>();
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

