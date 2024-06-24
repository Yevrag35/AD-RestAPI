namespace AD.Api.Core.Ldap.Filters
{
    public enum FilterTokenType
    {
        Begin = 0x0,
        StartKeyword,
        PropertyName,
        FilterValue,
        EndKeyword,
        End,
    }
}