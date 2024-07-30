using AD.Api.Binding.Attributes;
using AD.Api.Components;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Core.Ldap
{
    public abstract class RequestParameters
    {
        [Domain]
        public DomainQuery Info { get; set; }

        public OneOf<LdapConnection, IActionResult> ApplyConnection(IConnectionService connectionService)
        {
            string domain = this.Info.Domain;

            if (!connectionService.RegisteredConnections.TryGetValue(domain, out ConnectionContext? context))
            {
                string? origDom = this.Info.Domain;
                return new DomainNotFoundResult(origDom);
            }

            this.OnApplyingConnection(context);
            return context.CreateConnection(this.Info.DomainController, this.Info.RequiresSSL);
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

