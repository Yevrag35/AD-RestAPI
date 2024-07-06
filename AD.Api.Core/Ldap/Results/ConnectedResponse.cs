using AD.Api.Attributes.Services;
using System.ComponentModel;

namespace AD.Api.Core.Ldap.Results;

[DynamicDependencyRegistration]
public sealed class ConnectedResponse : IDisposable, IServiceProvider
{
    private bool _disposed;
    private LdapConnection _connection;
    private DirectoryResponse _response;
    private IServiceProvider _provider;

    public LdapConnection ActiveConnection => _connection;
    public string FoundObject { get; private set; }
    public DirectoryResponse LastResponse => _response;

    private ConnectedResponse()
    {
        this.FoundObject = string.Empty;
        _connection = null!;
        _response = null!;
        _provider = null!;
    }

    private void AddDependencies(DirectoryResponse response, LdapConnection connection, IServiceProvider provider, string distinguishedName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _connection = connection;
        _response = response;
        _provider = provider;
        this.FoundObject = distinguishedName ?? string.Empty;
    }

    internal static ConnectedResponse Continue(LdapConnection connection, DirectoryResponse response, IServiceProvider requestServices)
    {
        ConnectedResponse continued = requestServices.GetRequiredService<ConnectedResponse>();
        string dn = string.Empty;
        if (response is SearchResponse searchResponse && searchResponse.Entries.Count > 0)
        {
            dn = searchResponse.Entries[0].DistinguishedName;
        }

        continued.AddDependencies(response, connection, requestServices, dn);

        return continued;
    }

    public object? GetService(Type serviceType)
    {
        return _provider?.GetService(serviceType);
    }

    public bool TryGetResponse<T>([NotNullWhen(true)] out T? response) where T : DirectoryResponse
    {
        if (_response is T tResp)
        {
            response = tResp;
            return true;
        }
        else
        {
            response = null;
            return false;
        }
    }

    [DynamicDependencyRegistrationMethod]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private static void AddToServices(IServiceCollection services)
    {
        static ConnectedResponse create(IServiceProvider x) => new();
        services.AddScoped(create);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection?.Dispose();
            }

            _provider = null!;
            _connection = null!;
            _disposed = true;
        }
    }
}
