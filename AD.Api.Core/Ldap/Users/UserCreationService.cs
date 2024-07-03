using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Security;
using AD.Api.Core.Ldap.Results;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Frozen;
using System.DirectoryServices.Protocols;
using AD.Api.Spans;
using AD.Api.Statics;

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
            // return oneOf.Match(
            //     f0: success => new SidString((byte[])success[AttributeConstants.OBJECT_SID]),
            //     f1: error => OneOf<SidString>.FromT1(error));
        }

        if (oneOf.TryGetT1(out error, out ResultEntry? entry))
        {
            return error;
        }

        Span<char> chars = stackalloc char[target.UrlQueryLength + 1 + SidString.MaxSidStringLength];
        int pos = 0;
        createdAt.CopyToSlice(chars, ref pos);

        Span<byte> sidBytes = entry[AttributeConstants.OBJECT_SID] as byte[];
        int prePos = pos;
        pos += SidString.FormatSpan(chars.Slice(pos), sidBytes);

        string sidValue = chars.Slice(prePos, pos - prePos).ToString();

        if (target != DomainQuery.Default)
        {
            chars[pos++] = CharConstants.QUESTION;
            target.AppendAsQuery(chars.Slice(pos), out int written);
            pos += written;
        }

        return new CreatedResult(
            location: new string(chars.Slice(0, pos)),
            value: new {
                Domain = target.Domain,
                Dn = request.GetDistinguishedName().ToString(),
                ObjectSid = sidValue,
            }
        );
    }

    private static IReadOnlyDictionary<string, object?> GetAttributesFromRequest(CreateUserRequest request)
    {
        return request.TryGetAttributes(out IReadOnlyDictionary<string, object?>? attributes)
            ? attributes
            : FrozenDictionary<string, object?>.Empty;
    }
}
