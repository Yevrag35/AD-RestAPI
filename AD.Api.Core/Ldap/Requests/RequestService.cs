using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Web;
using AD.Api.Enums;
using AD.Api.Pooling;

namespace AD.Api.Core.Ldap;

public interface IRequestService
{
	IConnectionService Connections { get; }

	ObjEither<LdapConnection, DomainNotFoundResult> Connect(RequestParameters parameters);
	bool TryConnect(RequestParameters parameters, [NotNullWhen(true)] out LdapConnection? connection, [NotNullWhen(false)] out IActionResult? errorResult);

	bool TryConnect(string? domainKey, [NotNullWhen(true)] out LdapConnection? connection, [NotNullWhen(false)] out IActionResult? errorResult);

	IActionResult FindAll<T, TResponse>(RequestParameters<T, TResponse> parameters, IServiceProvider requestServices)
		where TResponse : SearchResponse
		where T : LdapRequest;
	IActionResult FindAll<T, TResponse>(RequestParameters<T, TResponse> parameters, ConnectedResponse continuation)
		where TResponse : SearchResponse
		where T : LdapRequest;
	IActionResult FindOne<T, TResponse>(RequestParameters<T, TResponse> parameters, IServiceProvider requestServices)
		where T : LdapRequest
		where TResponse : SearchResponse;

	ObjEither<ConnectedResponse, IActionResult> FindOneAndContinue<T, TResponse>(RequestParameters<T, TResponse> parameters)
		where TResponse : SearchResponse
		where T : LdapRequest;

	ObjEither<TResponse, IActionResult> SendForResponse<TResponse>([DisallowNull] DirectoryRequest request, LdapConnection connection)
		where TResponse : DirectoryResponse;
}

[DependencyRegistration(typeof(IRequestService), Lifetime = ServiceLifetime.Singleton)]
internal sealed class RequestService : IRequestService
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IEnumStrings<ResultCode> _enumStrings;
	public IConnectionService Connections { get; }

	[DebuggerStepThrough]
	public RequestService(IConnectionService connectionService, IEnumStrings<ResultCode> enumStrings)
	{
		_enumStrings = enumStrings;
		this.Connections = connectionService;
	}

	// CONNECTIONS
	[DebuggerStepThrough]
	public ObjEither<LdapConnection, DomainNotFoundResult> Connect(RequestParameters parameters)
	{
		return parameters.ApplyConnection(this.Connections);
	}
	[DebuggerStepThrough]
	public bool TryConnect(
		RequestParameters parameters,
		[NotNullWhen(true)] out LdapConnection? connection,
		[NotNullWhen(false)] out DomainNotFoundResult? errorResult)
	{
		var either = this.Connect(parameters);
		connection = either.IsT1 ? either.AsT1 : null;
		errorResult = either.IsT2 ? either.AsT2 : null;
	}
	[DebuggerStepThrough]
	public bool TryConnect(
		string? domainKey,
		[NotNullWhen(true)] out LdapConnection? connection,
		[NotNullWhen(false)] out IActionResult? errorResult)
	{
		if (!this.Connections.TryGetConnection(domainKey, out connection))
		{
			errorResult = new DomainNotFoundResult(domainKey);
			return false;
		}
		else
		{
			errorResult = null;
			return true;
		}
	}

	// SEARCH REQUESTS
	public IActionResult FindAll<T, TResponse>(
		RequestParameters<T, TResponse> parameters,
		IServiceProvider requestServices)
			where TResponse : SearchResponse
			where T : LdapRequest
	{
		if (!this.TryConnect(parameters, out LdapConnection? connection, out DomainNotFoundResult? error))
		{
			return error;
		}

		using (connection)
		{
			return this.SendSearchRequest<T, ResultEntryCollection, TResponse>(parameters, connection, requestServices, isMultiRequest: true);
		}
	}
	public IActionResult FindAll<T, TResponse>(
		RequestParameters<T, TResponse> parameters,
		ConnectedResponse continuation)
			where TResponse : SearchResponse
			where T : LdapRequest
	{
		return this.SendSearchRequest<T, ResultEntryCollection, TResponse>(parameters, continuation.ActiveConnection, continuation, isMultiRequest: true);
	}

	public IActionResult FindOne<T, TResponse>(
		RequestParameters<T, TResponse> parameters,
		IServiceProvider requestServices)
			where TResponse : SearchResponse
			where T : LdapRequest
	{
		var oneOf = parameters.ApplyConnection(this.Connections);
		if (oneOf.TryGetT1(out IActionResult? error, out LdapConnection? connection))
		{
			return error;
		}

		using (connection)
		{
			return this.SendSearchRequest<T, ResultEntry, TResponse>(parameters, connection, requestServices, isMultiRequest: false);
		}
	}
	public ObjEither<ConnectedResponse, IActionResult> FindOneAndContinue<T, TResponse>(RequestParameters<T, TResponse> parameters)
		where TResponse : SearchResponse
		where T : LdapRequest
	{
		var oneOf = parameters.ApplyConnection(this.Connections);
		if (oneOf.TryGetT1(out IActionResult? error, out LdapConnection? connection))
		{
			return new(error);
		}

		var responseOr = this.SendForResponse<TResponse>(parameters.Request, connection);
		if (responseOr.TryGetT1(out error, out TResponse? response))
		{
			connection.Dispose();
			return new(error);
		}

		return ConnectedResponse.Continue(connection, response, parameters.Info);
	}

	private IActionResult SendSearchRequest<T, TCollection, TResponse>(RequestParameters<T, TResponse> parameters, LdapConnection connection, IServiceProvider requestServices, bool isMultiRequest)
		where T : LdapRequest
		where TCollection : ISearchResultEntry
		where TResponse : SearchResponse
	{
		var oneOf = this.SendForResponse<TResponse>(parameters.Request, connection);
		//if (oneOf.TryGetT1(out IActionResult? error, out TResponse? response))
		if (oneOf.IsT2)
		{
			return oneOf.AsT2;
		}

		TCollection collection = requestServices.GetRequiredService<IPooledItem<TCollection>>().Value;
		if (!collection.TryApplyResponse(parameters.Info.Domain, oneOf.AsT1))
		{
			return SendCustomExceptionResult(oneOf.AsT1, isMultiRequest);
		}

		if (!isMultiRequest)
		{
			return new OkObjectResult(collection);
		}

		CollectionResponse respCol = requestServices.GetRequiredService<CollectionResponse>();
		respCol.SetData(oneOf.AsT1, resultEntries: collection);

		return respCol;
	}

	// REQUEST SENDING
	public ObjEither<TResponse, IActionResult> SendForResponse<TResponse>(
		[DisallowNull] DirectoryRequest request,
		LdapConnection connection)
			where TResponse : DirectoryResponse
	{
		try
		{
			return (TResponse)connection.SendRequest(request);
		}
		catch (DirectoryOperationException operationsEx)
		{
			return new OperationsExceptionResult(operationsEx);
		}
		catch (LdapException ldapEx)
		{
			return new LdapExceptionResult(ldapEx);
		}
		catch (Exception otherEx)
		{
			return new ObjectResult(new
			{
				Result = ResultCode.Other,
				ResultCode = otherEx.HResult,
				Error = otherEx.Message ?? "No message provided.",
			})
			{
				StatusCode = StatusCodes.Status500InternalServerError,
			};
		}
	}

	// EXCEPTION HANDLING
	private static ApiBadRequestResult SendCustomExceptionResult(SearchResponse response, bool isMultiRequest)
	{
		bool isZero = response.Entries.Count == 0;

		ResultCode code = isMultiRequest
			? ResultCode.Other
			: !isZero
				? ResultCode.ResultsTooLarge
				: ResultCode.NoSuchObject;

		string message = isMultiRequest
			? "The request failed to apply the response for serialization."
			: isZero
				? "The requested object was not found in the directory."
				: $"The wrong amount of results were returned. Expected one (1) result and got {response.Entries.Count}.";

		return new ApiBadRequestResult(message, code);
	}
}

