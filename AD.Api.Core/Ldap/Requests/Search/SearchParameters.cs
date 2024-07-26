using AD.Api.Pooling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using SR = System.DirectoryServices.Protocols.SearchRequest;

namespace AD.Api.Core.Ldap;

public class SearchParameters : RequestParameters<LdapSearchRequest, SearchResponse>, IValidatableObject
{
    private static readonly FrozenSet<string> _descOrder =
        FrozenSet.ToFrozenSet(["desc", "descending", "1"], StringComparer.OrdinalIgnoreCase);

    private IPooledItem<LdapSearchRequest> _searchRequest = null!;

    internal ISearchFilter? BackingFilter { get; set; }

    [FromQuery(Name = "scope")]
    public SearchScope? Scope { get; set; }

    [FromQuery(Name = "properties")]
    public string? Properties { get; set; }

    [BindNever]
    public string[] PropertiesArray { get; set; } = [];

    [FromQuery(Name = "limit")]
    [Range(0, int.MaxValue)]
    public int? SizeLimit { get; set; }

    [FromQuery(Name = "sortDir")]
    [AllowedValues("asc", "desc", "0", "1", "ascending", "descending", "", null)]
    public string? SortDirection { get; set; }

    [FromQuery(Name = "sortBy")]
    public string? SortProperty { get; set; }

    [FromServices]
    public required IPooledItem<LdapSearchRequest> SearchRequest
    {
        get => _searchRequest;
        set
        {
            Guid id = value.Value.RequestId;
            value.Value.Reset();
            value.Value.RequestId = id;
            _searchRequest = value;
        }
    }

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
        else if (this.PropertiesArray.Length > 0)
        {
            this.SearchRequest.Value.AddAttributes(this.PropertiesArray, searchFilter.RequestBaseType);
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

        if (searchFilter.SearchBase.HasValue && !searchFilter.SearchBase.Value.IsEmpty)
        {
            request.DistinguishedName = searchFilter.SearchBase.Value.ToString();
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
    public void SetProperties(string mainProperty, string[]? extraProperties)
    {
        if (extraProperties is null || extraProperties.Length == 0)
        {
            this.PropertiesArray = [];
            this.Properties = mainProperty;
            return;
        }
        else if (extraProperties.Contains(mainProperty, StringComparer.OrdinalIgnoreCase))
        {
            this.Properties = null;
            this.PropertiesArray = extraProperties;
        }

        string[] atts = new string[extraProperties.Length + 1];
        atts[0] = mainProperty;
        extraProperties.CopyTo(atts, 1);

        this.Properties = null;
        this.PropertiesArray = atts;
    }

    public static implicit operator SR(SearchParameters parameters)
    {
        return parameters.SearchRequest.Value.AsLdapRequest();
    }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        IConnectionService conSvc = validationContext.GetRequiredService<IConnectionService>();
        if (!conSvc.RegisteredConnections.ContainsKey(this.Info.Domain))
        {
            yield return new ValidationResult($"'{this.Info.Domain}' - Domain not found", [nameof(this.Info.Domain)]);
        }
    }
}