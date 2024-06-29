using AD.Api.Binding.Attributes;
using AD.Api.Components;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap
{
    public abstract class RequestParameters
    {
        [QueryDomain]
        public virtual string? Domain { get; set; }

        [MemberNotNull(nameof(Domain))]
        public OneOf<LdapConnection, IActionResult> ApplyConnection(IConnectionService connectionService)
        {
            string domain = this.Domain ?? string.Empty;

            if (!connectionService.RegisteredConnections.TryGetValue(domain, out ConnectionContext? context))
            {
                string? origDom = this.Domain;
                this.Domain = domain;
                return new DomainNotFoundResult(origDom);
            }

            this.Domain = domain;
            this.OnApplyingConnection(context);
            return context.CreateConnection();
        }

        protected abstract void OnApplyingConnection(ConnectionContext context);
    }

    public abstract class RequestParameters<T, TResponse> : RequestParameters
        where T : LdapRequest
        where TResponse : DirectoryResponse
    {
        public abstract T Request { get; }
    }
}

