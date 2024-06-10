using AD.Api.Attributes.Services;
using System.Collections.Concurrent;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public interface IConnectionPool
    {

    }

    [DependencyRegistration(typeof(IConnectionPool), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class ConnectionPool : IConnectionPool
    {
        const int MAX = 10;
        private readonly ConcurrentBag<LdapConnection> _bag;

        public ConnectionPool()
        {
            _bag = [];
        }

        public void Return(LdapConnection connection)
        {
            if (connection is null)
            {
                return;
            }

            //connection.SessionOptions.
            if (_bag.Count < MAX)
            {
                _bag.Add(connection);
            }
        }
        public bool TryTake([NotNullWhen(true)] out LdapConnection? connection)
        {
            return _bag.TryTake(out connection);
        }
    }
}

