using AD.Api.Attributes.Services;
using AD.Api.Components;
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
        OneOf<SidString, IActionResult> Create(string? domainKey, CreateUserRequest request, string? domainController = null);
    }

    [DependencyRegistration(typeof(IUserCreations), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class UserCreationService : CreationService, IUserCreations
    {
        public UserCreationService(WellKnownObjectDictionary wellKnowns, IRequestService requests)
            : base(wellKnowns, requests)
        {
        }

        [SupportedOSPlatform("WINDOWS")]
        public OneOf<SidString, IActionResult> Create(string? domainKey, CreateUserRequest request, string? domainController = null)
        {
            var conOneOf = this.Requests.Connections.GetConnection(domainKey, domainController);
            if (conOneOf.TryGetT1(out IActionResult? error, out LdapConnection? connection))
            {
                return OneOf<SidString>.FromT1(error);
            }

            using (connection)
            {
                IReadOnlyDictionary<string, object?> attributes = GetAttributesFromRequest(request);

                var oneOf = this.SendRequest(connection, domainKey, request, attributes);
                return oneOf.Match(
                    f0: success => new SidString((byte[])success[AttributeConstants.OBJECT_SID]),
                    f1: error => OneOf<SidString>.FromT1(error));
            }
        }

        private static IReadOnlyDictionary<string, object?> GetAttributesFromRequest(CreateUserRequest request)
        {
            return request.TryGetAttributes(out IReadOnlyDictionary<string, object?>? attributes)
                ? attributes
                : FrozenDictionary<string, object?>.Empty;
        }
    }
}
