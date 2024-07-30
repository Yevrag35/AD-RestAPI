using AD.Api.Components;
using AD.Api.Core.Extensions.Results;
using AD.Api.Core.Ldap.Results;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Frozen;

namespace AD.Api.Core.Ldap.Users;

public partial interface IUserService
{
    IActionResult Create(in DomainQuery target, CreateUserRequest request, [ConstantExpected] string createdAt);
}

internal sealed partial class UserService
{
    public IActionResult Create(in DomainQuery target, CreateUserRequest request, [ConstantExpected] string createdAt)
    {
        var conOneOf = _requestSvc.Connections.GetConnection(in target);
        if (conOneOf.TryGetT1(out IActionResult? error, out LdapConnection? connection))
        {
            return error;
        }

        OneOf<ResultEntry, IActionResult> oneOf;
        using (connection)
        {
            IReadOnlyDictionary<string, object?> attributes = GetAttributesFromRequest(request);

            oneOf = _creationSvc.SendRequest(connection, in target, request, attributes);
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