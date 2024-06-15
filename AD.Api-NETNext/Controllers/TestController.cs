using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Requests.Search;
using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Ldap.Services.Schemas;
using AD.Api.Pooling;
using AD.Api.Strings.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AD.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        const string DN = "CN=Edgard Oliva,OU=Real Users,DC=yevrag35,DC=com";

        public IConnectionService Connections { get; }

        public TestController(IConnectionService connections)
        {
            this.Connections = connections;
        }

        [HttpGet]
        [SupportedOSPlatform("WINDOWS")]
        public IActionResult GetConnection(
            [FromServices] ResultEntry entry,
            [FromServices] IPooledItem<LdapSearchRequest> request,
            [FromQuery] string? domain = null,
            [FromQuery] string? properties = null)
        {
            if (!string.IsNullOrWhiteSpace(properties))
            {
                request.Value.AddAttributes(properties);
            }

            this.HttpContext.Response.Headers.Append("X-Ldap-Request-Id", request.Value.GetRequestId());
            if (!request.Value.ApplyConnection(ref domain, this.Connections, out var connection))
            {
                return this.BadRequest();
            }

            using (connection)
            {
                request.Value.Filter = "(&(objectClass=user)(objectCategory=person)(cn=Edgard Oliva))";

                var response = (SearchResponse)connection.SendRequest(request.Value);
                SearchResultEntry searchEntry = response.Entries[0];
                entry.AddResult(domain, searchEntry);

                return this.Ok(new
                {
                    Result = response.ResultCode,
                    ResultCode = (int)response.ResultCode,
                    Data = entry,
                });
            }
        }

        //[HttpPost("disable")]
        //public IActionResult TestDisable(
        //    [FromQuery] string? domain = null)
        //{
        //    if (!conSvc.TryGetConnection(domain, out LdapConnection? conn))
        //    {
        //        return this.BadRequest();
        //    }

        //    var mod = new ModifyRequest(DN, DirectoryAttributeOperation.Replace, "userAccountControl", "66050");
        //    var resp = (ModifyResponse)conn.SendRequest(mod);

        //    return this.Ok(new
        //    {
        //        result = resp.ResultCode,
        //        error = resp.ErrorMessage,
        //    });
        //    //using DirectoryEntry entry = new($"LDAP://{DN}");
        //    //entry.Properties["userAccountControl"].Value = 66050; // disable

        //    //entry.CommitChanges();

        //    //return this.Ok(new
        //    //{
        //    //    message = "changed"
        //    //});
        //}
    }
}
