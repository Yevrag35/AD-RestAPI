using AD.Api.Core.Ldap.Filters;

namespace AD.Api.Core.Ldap
{
    public interface ICreateRequest
    {
        FilteredRequestType RequestType { get; }
        IServiceProvider RequestServices { get; }
        bool HasPath { get; }

        DistinguishedName GetDistinguishedName();
    }
}

