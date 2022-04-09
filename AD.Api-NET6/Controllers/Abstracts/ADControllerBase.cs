using AD.Api.Ldap;
using AD.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers
{
    public abstract class ADControllerBase : ControllerBase
    {
        protected IConnectionService Connections { get; }
        protected ISerializationService SerializationService { get; }

        public ADControllerBase(IConnectionService connectionService, ISerializationService serializationService)
            : base()
        {
            this.Connections = connectionService;
            this.SerializationService = serializationService;
        }

        protected LdapConnection GetConnection(string? domain, string? searchBase = null)
        {
            return string.IsNullOrWhiteSpace(domain)
                ? this.Connections.GetDefaultConnection(searchBase)
                : this.Connections.GetConnection(domain, searchBase);
        }
    }
}
