namespace AD.Api.Core.Ldap.Requests.Search
{
    public sealed class CachedSearchParameters : SearchParameters
    {
        public byte[] CookieBytes { get; set; }
        public Guid ContinueKey { get; set; }

        [SetsRequiredMembers]
        public CachedSearchParameters(SearchParameters fromOther, byte[] cookie)
        {
            this.CookieBytes = cookie;
            this.Domain = fromOther.Domain;
            this.FilterSvc = fromOther.FilterSvc;
            this.PageSize = fromOther.PageSize;
            this.Properties = fromOther.Properties;
            this.Scope = fromOther.Scope;
            this.SizeLimit = fromOther.SizeLimit;
            this.SortDirection = fromOther.SortDirection;
            this.SortProperty = fromOther.SortProperty;
            this.SearchRequest = fromOther.SearchRequest;
            this.BackingFilter = fromOther.BackingFilter;
        }

        public override void ApplyParameters(ISearchFilter? searchFilter)
        {
            base.ApplyParameters(searchFilter);
            this.SearchRequest.Value.SetCookie(this.PageSize, this.CookieBytes);
        }
    }
}

