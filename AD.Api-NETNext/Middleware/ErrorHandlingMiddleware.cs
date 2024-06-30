using AD.Api.Core.Web;
using AD.Api.Enums;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using NLog;
using System.DirectoryServices.Protocols;
using System.Net;

namespace AD.Api.Middleware
{
    public sealed class ErrorHandlingMiddleware
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IEnumStrings<ResultCode> _errorCodes;

        public ErrorHandlingMiddleware(IEnumStrings<ResultCode> resultCodes)
        {
            _errorCodes = resultCodes;
        }

        public Task Invoke(HttpContext context)
        {
            _logger.Warn("Error handling invoked -> {PathAndQuery:l}", context.Request.GetEncodedPathAndQuery());
            Exception? ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex is null)
            {
                return Task.CompletedTask;
            }

            _logger.Error(ex);

            ResultCode code;
            int intCode;
            if (ex is LdapException ldapEx)
            {
                ResultCode possible = (ResultCode)ldapEx.ErrorCode;
                code = _errorCodes.ContainsEnum(possible)
                    ? possible
                    : ResultCode.OperationsError;

                intCode = ldapEx.ErrorCode;
            }
            else if (ex is DirectoryOperationException dirEx)
            {
                code = dirEx.Response.ResultCode;
                intCode = (int)code;
            }
            else
            {
                code = ResultCode.Other;
                intCode = ex.HResult;
            }

            ErrorBody body = new()
            {
                Message = ex.Message,
                Result = code,
                ResultCode = intCode,
            };

            return context.Response.WriteAsJsonAsync(body, context.RequestAborted);
        }
    }
}
