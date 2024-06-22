using System.Runtime.InteropServices;

namespace AD.Api.Core.Ldap
{
    public interface ISearchFilter
    {
        [MemberNotNullWhen(true, nameof(LdapFilter))]
        bool HasLdapFilter { get; }
        string? LdapFilter { get; }
        FilteredRequestType? RequestBaseType { get; }
        string? SearchBase { get; }
    }

    [DebuggerStepThrough]
    [StructLayout(LayoutKind.Auto)]
    public readonly struct SearchFilterLite : ISearchFilter
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool _hasFilter;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string? _filter;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly FilteredRequestType? _requestType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string? _searchBase;

        [MemberNotNullWhen(true, nameof(LdapFilter))]
        public bool HasLdapFilter => _hasFilter;
        public string? LdapFilter => _filter;
        public FilteredRequestType? RequestBaseType => _requestType;
        public string? SearchBase => _searchBase;

        private SearchFilterLite(string? filter, FilteredRequestType? requestType, string? searchBase)
        {
            _hasFilter = !string.IsNullOrWhiteSpace(filter);
            _filter = filter;
            _requestType = requestType;
            _searchBase = searchBase;
        }

        public static readonly SearchFilterLite Empty = new(null, null, null);

        public static SearchFilterLite Create(string filter, FilteredRequestType? types = null)
        {
            return Create(filter, null, types);
        }
        public static SearchFilterLite Create(string filter, string? searchBase, FilteredRequestType? types = null)
        {
            return new(filter, types, searchBase);
        }
    }
}

