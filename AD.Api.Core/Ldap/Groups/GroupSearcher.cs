using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Web;
using AD.Api.Pooling;
using AD.Api.Strings.Extensions;
using AD.Api.Strings.Spans;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;

namespace AD.Api.Core.Ldap.Groups;

public interface IGroupSearcher
{
    IActionResult ResolveUserGroups(ConnectedResponse continuation, string? propertyString);
    IActionResult ResolveUserGroups(ConnectedResponse continuation, string[]? properties);
}

internal sealed class GroupSearcher
{
    static readonly int MEMBER_LENGTH = AttributeConstants.MEMBER.Length + LdapConstants.RECURSIVE.Length;

    private readonly ILdapFilterService _filterSvc;
    private readonly IRequestService _requestSvc;

    public GroupSearcher(ILdapFilterService filterSvc, IRequestService requestSvc)
    {
        _filterSvc = filterSvc;
        _requestSvc = requestSvc;
    }

    public IActionResult ResolveUserGroups(ConnectedResponse continuation, string? propertyString)
    {
        if (string.IsNullOrWhiteSpace(propertyString))
        {
            return this.ResolveUserGroups(continuation, properties: null);
        }

        ReadOnlySpan<char> pChars = propertyString.Trim().AsSpan();
        Span<Range> ranges = stackalloc Range[pChars.Length];

        int written = pChars.SplitAny(ranges, [',', ' ', '+'], StringSplitOptions.RemoveEmptyEntries);
        string[] array = new string[written];
        for (int i = 0; i < written; i++)
        {
            array[i] = pChars[ranges[i]].ToString();
        }

        return this.ResolveUserGroups(continuation, array);
    }
    public IActionResult ResolveUserGroups(ConnectedResponse continuation, string[]? properties)
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
        
        SearchFilterLite searchFilter = SearchFilterLite.Create(groupFilter, FilteredRequestType.Group);
        SearchParameters parameters = new()
        {
            SearchRequest = continuation.GetRequiredService<IPooledItem<LdapSearchRequest>>(),
            Info = continuation.Target,
            Scope = SearchScope.Subtree,
        };

        parameters.SetProperties(AttributeConstants.DISTINGUISHED_NAME, properties);
        parameters.ApplyParameters(searchFilter);

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
        string[] memberOf = entry.Attributes[AttributeConstants.MEMBER_OF].OfType<string>().ToArray();

        return new ObjectResult(new
        {
            Count = memberOf.Length,
            Data = memberOf,
        });
    }
}