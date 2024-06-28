using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap.Requests.Creation.Users;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Security;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Frozen;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace AD.Api.Core.Ldap.Users
{
    public interface IUserCreations
    {
        [SupportedOSPlatform("WINDOWS")]
        OneOf<SecurityIdentifier, IActionResult> Create(string? domainKey, CreateUserRequest request);
    }

    [DependencyRegistration(typeof(IUserCreations), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class UserCreationService : CreationService, IUserCreations
    {
        private static readonly FrozenDictionary<string, object?> _noAttributes = FrozenDictionary<string, object?>.Empty;

        public UserCreationService(WellKnownObjectDictionary wellKnowns, IRequestService requests)
            : base(wellKnowns, requests)
        {
        }

        [SupportedOSPlatform("WINDOWS")]
        public OneOf<SecurityIdentifier, IActionResult> Create(string? domainKey, CreateUserRequest request)
        {
            if (!this.Requests.TryConnect(domainKey, out var connection, out var errorResult))
            {
                return OneOf<SecurityIdentifier>.FromT1(errorResult);
            }

            using (connection)
            {
                IReadOnlyDictionary<string, object?> attributes = GetAttributesFromRequest(request);

                var oneOf = this.SendRequest(connection, domainKey, request, attributes);
                return oneOf.Match(
                    f0: success => new SecurityIdentifier((byte[])success[AttributeConstants.OBJECT_SID], 0),
                    f1: error => OneOf<SecurityIdentifier>.FromT1(error));
            }
        }

        private static IReadOnlyDictionary<string, object?> GetAttributesFromRequest(CreateUserRequest request)
        {
            return request.TryGetAttributes(out IReadOnlyDictionary<string, object?>? attributes)
                ? attributes
                : _noAttributes;
        }
    }
}

