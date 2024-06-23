using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Web;
using AD.Api.Enums;
using AD.Api.Pooling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap
{
    public interface IRequestService
    {
        IConnectionService Connections { get; }

        OneOf<LdapConnection, IActionResult> Connect(RequestParameters parameters);
        bool TryConnect(RequestParameters parameters, [NotNullWhen(true)] out LdapConnection? connection, [NotNullWhen(false)] out IActionResult? errorResult);

        IActionResult SendRequestMulti<T, TResponse>(RequestParameters<T, TResponse> parameters, IServiceProvider requestServices)
            where TResponse : SearchResponse
            where T : LdapRequest;
        IActionResult SendRequestSingle<T, TResponse>(RequestParameters<T, TResponse> parameters, IServiceProvider requestServices)
            where T : LdapRequest
            where TResponse : SearchResponse;
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
        public OneOf<LdapConnection, IActionResult> Connect(RequestParameters parameters)
        {
            return parameters.ApplyConnection(this.Connections);
        }
        [DebuggerStepThrough]
        public bool TryConnect(RequestParameters parameters, [NotNullWhen(true)] out LdapConnection? connection, [NotNullWhen(false)] out IActionResult? errorResult)
        {
            return this.Connect(parameters).TryGetT0(out connection, out errorResult);
        }

        // SEARCH REQUESTS
        public IActionResult SendRequestMulti<T, TResponse>(RequestParameters<T, TResponse> parameters, IServiceProvider requestServices)
            where TResponse : SearchResponse
            where T : LdapRequest
        {
            if (!this.TryConnect(parameters, out LdapConnection? connection, out IActionResult? error))
            {
                return error;
            }

            using (connection)
            {
                return this.SendSearchRequest<T, ResultEntryCollection, TResponse>(parameters, connection, requestServices, isMultiRequest: true);
            }
        }
        public IActionResult SendRequestSingle<T, TResponse>(RequestParameters<T, TResponse> parameters, IServiceProvider requestServices)
            where TResponse : SearchResponse
            where T : LdapRequest
        {
            if (!this.TryConnect(parameters, out LdapConnection? connection, out IActionResult? error))
            {
                return error;
            }

            using (connection)
            {
                return this.SendSearchRequest<T, ResultEntry, TResponse>(parameters, connection, requestServices, isMultiRequest: false);
            }
        }

        private IActionResult SendSearchRequest<T, TCollection, TResponse>(RequestParameters<T, TResponse> parameters, LdapConnection connection, IServiceProvider requestServices, bool isMultiRequest)
            where T : LdapRequest
            where TCollection : ISearchResultEntry
            where TResponse : SearchResponse
        {
            var oneOf = SendForResponse<TResponse>(parameters.Request, connection);
            if (oneOf.TryGetT1(out IActionResult? error, out TResponse? response))
            {
                return error;
            }

            TCollection collection = requestServices.GetRequiredService<IPooledItem<TCollection>>().Value;
            if (!collection.TryApplyResponse(parameters.Domain, response))
            {
                return this.SendCustomExceptionResult(response, isMultiRequest);
            }

            if (!isMultiRequest)
            {
                return new OkObjectResult(collection);
            }

            CollectionResponse respCol = requestServices.GetRequiredService<CollectionResponse>();
            respCol.SetData(response, resultEntries: collection);

            return respCol;
        }

        // REQUEST SENDING
        private static OneOf<TResponse, IActionResult> SendForResponse<TResponse>([DisallowNull] DirectoryRequest request, LdapConnection connection)
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
        private ObjectResult SendCustomExceptionResult(SearchResponse response, bool isMultiRequest)
        {
            int statusCode = isMultiRequest
                    ? StatusCodes.Status500InternalServerError
                    : StatusCodes.Status400BadRequest;

            ResultCode code = isMultiRequest
                ? ResultCode.Other
                : response.Entries.Count > 0
                    ? ResultCode.ResultsTooLarge
                    : ResultCode.NoSuchObject;

            string message = isMultiRequest
                ? "The request failed to apply the response for serialization."
                : $"The wrong amount of results were returned. Excepted only 1 result and got {response.Entries.Count}.";

            return new ObjectResult(new
            {
                Error = message,
                ResultCode = (int)code,
                Result = _enumStrings[code],
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
    }
}

