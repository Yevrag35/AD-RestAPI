using AD.Api.Attributes.Services;
using System.ComponentModel;

namespace AD.Api.Core.Ldap.Results;

/// <summary>
/// A class that represents a response from a directory search operation which includes the active connection that
/// was used to perform the search for sending further queries.
/// </summary>
[DynamicDependencyRegistration]
public sealed class ConnectedResponse : IDisposable, IServiceProvider
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private LdapConnection _connection;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SearchResponse _response;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ResultCode? _responseResultCode;

	private bool _disposed;
	private DomainQuery _target;

	/// <summary>
	/// The active connection that was used to perform the search.
	/// </summary>
	/// <remarks>
	/// It remains open and connected for sending further queries.
	/// </remarks>
	public LdapConnection ActiveConnection => _connection;
	/// <summary>
	/// The distinguished name of the single found object from the initial search operation.
	/// </summary>
	public DistinguishedName FoundObject { get; private set; }
	/// <summary>
	/// Indicates whether there is a <see cref="SearchResponse"/> provided to the this <see cref="ConnectedResponse"/>
	/// and it's result code is <see cref="ResultCode.Success"/>.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if there is a <see cref="SearchResponse"/> provided and it's result code is
	/// <see cref="ResultCode.Success"/>; otherwise, if not or no response was provided, <see langword="false"/>.
	/// </returns>
	[MemberNotNullWhen(true, nameof(ResultEntry))]
	public bool IsSearchSuccess
	{
		get
		{
			return _responseResultCode.HasValue
				&& ResultCode.Success == _responseResultCode.Value
				&& !this.FoundObject.IsEmpty;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public SearchResponse LastResponse => _response;
	/// <summary>
	/// 
	/// </summary>
	public SearchResultEntry? ResultEntry => 0 < _response?.Entries.Count ? _response.Entries[0] : null;
	/// <summary>
	/// 
	/// </summary>
	public DomainQuery Target => _target;

	private ConnectedResponse()
	{
		this.FoundObject = DistinguishedName.Empty;
		_connection = null!;
		_response = null!;
		_target = DomainQuery.Default;
	}

	private void AddDependencies(SearchResponse response, LdapConnection connection, in DomainQuery target, string distinguishedName)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		_connection = connection;
		_response = response;
		_responseResultCode = response.ResultCode;
		_target = target;

		this.FoundObject = !string.IsNullOrWhiteSpace(distinguishedName)
			? DistinguishedName.Parse(distinguishedName)
			: DistinguishedName.Empty;
	}

	internal static ConnectedResponse Continue(LdapConnection connection, SearchResponse response, DomainQuery target)
	{
		ConnectedResponse continued = target.GetRequiredService<ConnectedResponse>();
		string dn = string.Empty;
		if (response.Entries.Count > 0)
		{
			dn = response.Entries[0].DistinguishedName;
		}

		continued.AddDependencies(response, connection, in target, dn);

		return continued;
	}

	/// <inheritdoc/>
	public object? GetService(Type serviceType)
	{
		return _target.GetService(serviceType);
	}

	//public bool TryGetResponse<T>([NotNullWhen(true)] out T? response) where T : DirectoryResponse
	//{
	//    if (_response is T tResp)
	//    {
	//        response = tResp;
	//        return true;
	//    }
	//    else
	//    {
	//        response = null;
	//        return false;
	//    }
	//}

	[DynamicDependencyRegistrationMethod]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private static void AddToServices(IServiceCollection services)
	{
		static ConnectedResponse create(IServiceProvider x) => new();
		services.AddScoped(create);
	}

	/// <summary>
	/// Closes and disposes of the underlying connection for this <see cref="ConnectedResponse"/>.
	/// </summary>
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

			_target = DomainQuery.Default;
			_connection = null!;
			_disposed = true;
		}
	}
}
