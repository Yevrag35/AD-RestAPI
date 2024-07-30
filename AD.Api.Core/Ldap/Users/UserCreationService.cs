using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Security;
using AD.Api.Core.Ldap.Results;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Frozen;
using System.DirectoryServices.Protocols;
using AD.Api.Spans;
using AD.Api.Statics;
using AD.Api.Core.Extensions.Results;

namespace AD.Api.Core.Ldap.Users;

public interface IUserCreations
{
    IActionResult Create(in DomainQuery target, CreateUserRequest request, [ConstantExpected] string createdAt);
}

[DependencyRegistration(typeof(IUserCreations), Lifetime = ServiceLifetime.Singleton)]
internal sealed class UserCreationService : CreationService, IUserCreations
{
    public UserCreationService(WellKnownObjectDictionary wellKnowns, IRequestService requests)
        : base(wellKnowns, requests)
    {
    }

    public IActionResult Create(in DomainQuery target, CreateUserRequest request, [ConstantExpected] string createdAt)
    {
        var conOneOf = this.Requests.Connections.GetConnection(in target);
        if (conOneOf.TryGetT1(out IActionResult? error, out LdapConnection? connection))
        {
            return error;
        }

        OneOf<ResultEntry, IActionResult> oneOf;
        using (connection)
        {
            IReadOnlyDictionary<string, object?> attributes = GetAttributesFromRequest(request);

            oneOf = this.SendRequest(connection, in target, request, attributes);
        }

        if (oneOf.TryGetT1(out error, out ResultEntry? entry))
        {
            return error;
        }

        CreatedObject createObj = entry.ToCreatedObject(createdAt, in target);
        return new CreatedResult(createObj.Location, createObj);
    }

    private static IReadOnlyDictionary<string, object?> GetAttributesFromRequest(CreateUserRequest request)
    {
        return request.TryGetAttributes(out IReadOnlyDictionary<string, object?>? attributes)
            ? attributes
            : FrozenDictionary<string, object?>.Empty;
    }
}
