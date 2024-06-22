﻿using AD.Api.Binding.Attributes;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Security;
using AD.Api.Pooling;
using AD.Api.Strings.Spans;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;

namespace AD.Api.Controllers.Users
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public IConnectionService Connections { get; }
        public ILdapFilterService Filters { get; }

        public UserController(IConnectionService connectionService, ILdapFilterService filters)
        {
            this.Connections = connectionService;
            this.Filters = filters;
        }

        [HttpGet]
        [Route("{sid:objectsid}")]
        public IActionResult GetUser(
            [FromServices] IPooledItem<ResultEntry> result,
            [FromQuery] SearchParameters parameters,
            [FromRouteSid] SidString sid)
        {
            LdapConnection connection = parameters.ApplyConnection(this.Connections);
            SpanStringBuilder builder = new(stackalloc char[SidString.MaxSidStringLength + 44]);
            builder = builder.Append(['(', '&'])
                             .Append("(objectSid=")
                             .Append(SidString.MaxSidStringLength, sid, (chars, state) =>
                              {
                                  state.TryFormat(chars, out int written);
                                  return written;
                              })
                             .Append(')')
                             .Append(this.Filters.GetFilter(FilteredRequestType.User, addEnclosure: false))
                             .Append(')');

            SearchFilterLite searchFilter = SearchFilterLite.Create(builder.Build(), FilteredRequestType.User);
            parameters.ApplyParameters(searchFilter);

            var response = (SearchResponse)connection.SendRequest(parameters);
            if (response.ResultCode != ResultCode.Success || response.Entries.Count > 1)
            {
                return this.BadRequest(new
                {
                    Result = response.ResultCode,
                    ResultCode = (int)response.ResultCode,
                    Message = response.ErrorMessage ?? "More than one entry was found.",
                });
            }

            result.Value.AddResult(parameters.Domain, response.Entries[0]);
            return this.Ok(result.Value);
        }
    }
}
