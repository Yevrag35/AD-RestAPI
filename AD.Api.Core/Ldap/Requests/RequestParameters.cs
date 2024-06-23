using AD.Api.Components;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Core.Ldap
{
    public abstract class RequestParameters
    {
        [FromQuery]
        public virtual string? Domain { get; set; }

        [MemberNotNull(nameof(Domain))]
        public OneOf<LdapConnection, IActionResult> ApplyConnection(IConnectionService connectionService)
        {
            string domain = this.Domain ?? string.Empty;

            OneOf<LdapConnection, IActionResult> oneOf = !connectionService.RegisteredConnections
                .TryGetValue(domain, out ConnectionContext? context)
                    ? new DomainNotFoundResult(this.Domain)
                    : context.CreateConnection();

            this.Domain = domain;
            return oneOf;
        }
    }

    public abstract class RequestParameters<T, TResponse> : RequestParameters
        where T : LdapRequest
        where TResponse : DirectoryResponse
    {
        public abstract T Request { get; }
    }
}

