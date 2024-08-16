using AD.Api.Attributes.Services;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Web;
using AD.Api.Pooling;
using AD.Api.Strings.Extensions;
using AD.Api.Strings.Spans;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;

namespace AD.Api.Core.Ldap.Groups;

public interface IGroupService
{
    IActionResult ResolveUserGroups(ConnectedResponse continuation, string? propertyString, int sizeLimit);
    IActionResult ResolveUserGroups(ConnectedResponse continuation, string[]? properties, int sizeLimit);
    IActionResult ResolveUserGroups(ConnectedResponse continuation, SearchParameters searchParameters);
}

[DependencyRegistration(typeof(IGroupService), Lifetime = ServiceLifetime.Singleton)]
internal sealed class GroupService : IGroupService
{
    static readonly int MEMBER_LENGTH = AttributeConstants.MEMBER.Length + LdapConstants.RECURSIVE.Length;

    private readonly IConnectionService _connectSvc;
    private readonly ILdapFilterService _filterSvc;
    private readonly IRequestService _requestSvc;

    public GroupService(IConnectionService connectSvc, ILdapFilterService filterSvc, IRequestService requestSvc)
    {
        _connectSvc = connectSvc;
        _filterSvc = filterSvc;
        _requestSvc = requestSvc;
    }

    public IActionResult ResolveUserGroups(ConnectedResponse continuation, string? propertyString, int sizeLimit)
    {
        if (string.IsNullOrWhiteSpace(propertyString))
        {
            return this.ResolveUserGroups(continuation, properties: null, sizeLimit);
        }

        ReadOnlySpan<char> pChars = propertyString.Trim().AsSpan();
        Span<Range> ranges = stackalloc Range[pChars.Length];

        int written = pChars.SplitAny(ranges, [',', ' ', '+'], StringSplitOptions.RemoveEmptyEntries);
        string[] array = new string[written];
        for (int i = 0; i < written; i++)
        {
            array[i] = pChars[ranges[i]].ToString();
        }

        return this.ResolveUserGroups(continuation, array, sizeLimit);
    }
    public IActionResult ResolveUserGroups(ConnectedResponse continuation, string[]? properties, int sizeLimit)
    {
        if (!continuation.IsSearchSuccess)
        {
            return new ApiBadRequestResult("The distinguished name of the user object was not found.", ResultCode.NoSuchObject);
        }
        else if (continuation.ResultEntry.Attributes.Contains(AttributeConstants.MEMBER_OF) && (properties is null || properties.Length == 0 || (properties.Length == 1 && AttributeConstants.DISTINGUISHED_NAME.Equals(properties[0], StringComparison.OrdinalIgnoreCase))))
        {
            return SendResultAsDNList(continuation.ResultEntry, continuation.LastResponse);
        }

        string groupFilter = GetGroupFilter(continuation.FoundObject, _filterSvc);
        
        //SearchFilterLite searchFilter = SearchFilterLite.Create(groupFilter, FilteredRequestType.Group);
        SearchParameters parameters = new()
        {
            SearchRequest = continuation.GetRequiredService<IPooledItem<LdapSearchRequest>>(),
            BackingFilter = new SearchFilter
            {
                Filter = groupFilter,
                SearchBase = DistinguishedName.Parse(_connectSvc.RegisteredConnections[continuation.Target.Domain]
            .DefaultNamingContext),
                SizeLimit = sizeLimit,
            },
            Info = continuation.Target,
            Scope = SearchScope.Subtree,
            SizeLimit = sizeLimit,
        };

        parameters.SetProperties(AttributeConstants.DISTINGUISHED_NAME, properties);
        parameters.ApplyParameters(null);

        return _requestSvc.FindAll(parameters, continuation);
    }
    public IActionResult ResolveUserGroups(ConnectedResponse continuation, SearchParameters parameters)
    {
        if (!parameters.Scope.HasValue)
        {
            parameters.Scope = SearchScope.Subtree;
        }

        if (!continuation.IsSearchSuccess)
        {
            return new ApiBadRequestResult("The distinguished name of the user object was not found.", ResultCode.NoSuchObject);
        }
        else if (continuation.ResultEntry.Attributes.Contains(AttributeConstants.MEMBER_OF) && (parameters.Properties.Length == 0 || (parameters.Properties.Length == 1 && AttributeConstants.DISTINGUISHED_NAME.Equals(parameters.Properties[0], StringComparison.OrdinalIgnoreCase))))
        {
            return SendResultAsDNList(continuation.ResultEntry, continuation.LastResponse);
        }

        parameters.SizeLimit ??= 0;
        parameters.BackingFilter = new SearchFilter
        {
            Filter = GetGroupFilter(continuation.FoundObject, _filterSvc),
            SearchBase = DistinguishedName.Parse(_connectSvc.RegisteredConnections[continuation.Target.Domain].DefaultNamingContext),
            SizeLimit = parameters.SizeLimit,
        };

        parameters.ApplyParameters(null);
        return _requestSvc.FindAll(parameters, continuation);
    }

    private static string GetGroupFilter(DistinguishedName userDn, ILdapFilterService filterSvc)
    {
        FilterSpanWriter writer = new(userDn.Length + MEMBER_LENGTH + 10);
        writer.Equal(AttributeConstants.MEMBER, LdapConstants.RECURSIVE, userDn, userDn.Length);

        string filter = filterSvc.AddToFilter(writer.AsSpan(), FilteredRequestType.Group, true);
        writer.Dispose();

        return filter;
    }
    private static ObjectResult SendResultAsDNList(SearchResultEntry entry, SearchResponse response)
    {
        string[] memberOf = entry.Attributes[AttributeConstants.MEMBER_OF].GetStringArray();

        return new ObjectResult(new
        {
            Count = memberOf.Length,
            Data = memberOf,
        });
    }

    private readonly record struct SearchFilter : ISearchFilter
    {
        public bool HasLdapFilter => true;
        [NotNull]
        public required string? Filter { get; init; }
        public string[]? Properties => null;
        [NotNull]
        public FilteredRequestType? RequestBaseType => FilteredRequestType.Group;
        [NotNull]
        public SearchScope? Scope => SearchScope.Subtree;
        [NotNull]
        public required DistinguishedName? SearchBase { get; init; }
        public required int? SizeLimit { get; init; }
        public string? SortBy { get; init; }
        public string? SortDirection { get; init; }
    }
}