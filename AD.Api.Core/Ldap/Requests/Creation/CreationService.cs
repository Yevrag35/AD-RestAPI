using AD.Api.Attributes;
using AD.Api.Components;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Web;
using AD.Api.Enums;
using AD.Api.Pooling;
using AD.Api.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap
{
    internal abstract class CreationService
    {
        private readonly NonLeaseablePool<object?[]> _objPool;

        protected IRequestService Requests { get; }
        protected WellKnownObjectDictionary WellKnowns { get; }

        protected CreationService(WellKnownObjectDictionary wellKnowns, IRequestService requests)
        {
            _objPool = new(5, PreFillBag(2), GetObjectArray, null);
            this.Requests = requests;
            this.WellKnowns = wellKnowns;
        }

        protected OneOf<ResultEntry, IActionResult> SendRequest<T>(LdapConnection connection, string? domainKey, ICreateRequest request, IReadOnlyDictionary<string, T> attributeValues)
        {
            string objClass = request.RequestType.GetObjectClass();
            bool needsObj = string.Empty.Equals(objClass);
            bool containsClass = attributeValues.ContainsKey(AttributeConstants.OBJECT_CLASS);
            if (needsObj && !containsClass)
            {
                return new ApiBadRequestResult(
                    "Unable to determine the object class for this request - you may have to specify it manually.", ResultCode.ObjectClassViolation);
            }

            DistinguishedName dn = request.GetDistinguishedName();
            if (!request.HasPath && !this.TryUpdateWithWellKnown(dn, domainKey, request.RequestType, out IActionResult? error))
            {
                return OneOf<ResultEntry>.FromT1(error);
            }

            AddRequest addRequest = this.CreateAddRequest(dn, attributeValues);
            if (!containsClass)
            {
                addRequest.Attributes.Add(new DirectoryAttribute(AttributeConstants.OBJECT_CLASS, objClass));
            }

            var oneOf = this.Requests.SendForResponse<AddResponse>(addRequest, connection);
            if (oneOf.TryGetT1(out error, out var success))
            {
                return OneOf<ResultEntry>.FromT1(error);
            }

            var filterSvc = request.RequestServices.GetRequiredService<ILdapFilterService>();
            string filter = filterSvc.GetFilter(request.RequestType, addEnclosure: true);

            SearchRequest search = new(dn.ToString(), filter, SearchScope.Base, [AttributeConstants.OBJECT_SID]);

            var searchOneOf = this.Requests.SendForResponse<SearchResponse>(search, connection);
            if (searchOneOf.TryGetT1(out error, out var searchSuccess))
            {
                return OneOf<ResultEntry>.FromT1(error);
            }
            else if (searchSuccess.Entries.Count == 0)
            {
                return new ObjectResult(new
                {
                    Error = "The object was seemingly created, but could not be found in the directory.",
                    ResultCode = (int)ResultCode.NoSuchObject,
                    Result = ResultCode.NoSuchObject,
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                };
            }

            var entry = request.RequestServices.GetRequiredService<IPooledItem<ResultEntry>>();
            entry.Value.AddResult(domainKey ?? string.Empty, searchSuccess.Entries[0]);

            return entry.Value;
        }

        private AddRequest CreateAddRequest<T>(DistinguishedName dn, IEnumerable<KeyValuePair<string, T>> attributeValues)
        {
            object?[] values = _objPool.Get();
            try
            {
                AddRequest addRequest = new(dn.ToString(), attributes: []);

                foreach (var kvp in attributeValues)
                {
                    DirectoryAttribute dirAttribute = kvp.ToDirectoryAttribute(values);
                    addRequest.Attributes.Add(dirAttribute);
                }

                return addRequest;
            }
            finally
            {
                _objPool.Return(values);
            }
        }
        private static object?[] GetObjectArray()
        {
            return new object[1];
        }
        private static IEnumerable<object?[]> PreFillBag(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return GetObjectArray();
            }
        }
        private bool TryUpdateWithWellKnown(DistinguishedName dn, string? domainKey, FilteredRequestType type, [NotNullWhen(false)] out IActionResult? errorResult)
        {
            if (!this.WellKnowns.TryGetValue(domainKey, requestType: type, out string? location))
            {
                errorResult = new ApiBadRequestResult(
                    "This request requires a path to be specified.", ResultCode.InvalidDNSyntax);

                return false;
            }

            errorResult = null;
            dn.Path = location;
            return true;
        }
    }
}

