using AD.Api.Pooling;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap
{
    public class SearchParameters : IValidatableObject
    {
        private string? _domain;

        [FromQuery(Name = "domain")]
        public string? Domain
        {
            get => _domain;
            set => _domain = value;
        }

        [FromQuery(Name = "searchBase")]
        public SearchScope? Scope { get; set; }

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
        public IPooledItem<LdapSearchRequest> SearchRequest { get; set; } = null!;

        [MemberNotNull(nameof(Domain))]
        public LdapConnection ApplyConnection(IConnectionService connectionService)
        {
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
            return this.SearchRequest.Value.ApplyConnection(ref _domain, connectionService);
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
        }

        public void ApplyParameters(string? filter)
        {
            this.SearchRequest.Value.AddAttributes(this.Properties);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                this.SearchRequest.Value.Filter = filter;
            }
        }

        public static implicit operator SearchRequest(SearchParameters parameters)
        {
            return parameters.SearchRequest.Value;
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

