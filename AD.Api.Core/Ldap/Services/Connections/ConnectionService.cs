using AD.Api.Attributes.Services;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public interface IConnectionService
    {

    }

    [DependencyRegistration(typeof(IConnectionService), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class ConnectionService : IConnectionService
    {
        public ConnectionService()
        {

        }
    }
}

