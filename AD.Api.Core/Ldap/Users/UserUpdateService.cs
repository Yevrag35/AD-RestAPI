using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Schema;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;
using System.Text.Json;

namespace AD.Api.Core.Ldap.Users
{
    public sealed class UserUpdateService
    {
        private readonly IRequestService _requestSvc;
        private readonly ISchemaService _schemaSvc;

        public UserUpdateService(IRequestService requestSvc, ISchemaService schemaSvc)
        {
            _requestSvc = requestSvc;
            _schemaSvc = schemaSvc;
        }

        public IActionResult UpdateUser(ConnectedResponse response, in DomainQuery target, IReadOnlyDictionary<string, JsonElement> updates)
        {
            if (string.IsNullOrWhiteSpace(response.FoundObject) || updates.Count <= 0)
            {
                return new ApiBadRequestResult("No updates provided or no user found.", ResultCode.NoSuchObject);
            }

            
        }
    }
}

