using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Results
{
    public interface ISearchResultEntry
    {
        int Count { get; }
        Guid LeaseId { get; }

        bool TryApplyResponse(string? domain, [NotNullWhen(true)] SearchResponse? response);
    }
}

