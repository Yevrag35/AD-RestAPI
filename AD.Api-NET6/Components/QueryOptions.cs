namespace AD.Api.Ldap.Search
{
    public record QueryOptions : SearchOptions
    {
        public string? Domain { get; init; }
        public string? SearchBase { get; init; }
    }
}
