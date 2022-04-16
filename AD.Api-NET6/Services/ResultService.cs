﻿using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using System.DirectoryServices;

namespace AD.Api.Services
{
    public interface IResultService
    {
        object GetQueryReply<T>(List<T> results, int limitedTo, IList<string>? propertiesRequested, string host, string? ldapFilter);
        ISuccessResult GetSuccess();
    }

    public class ResultService : IResultService
    {
        private static readonly Lazy<OperationResult> SuccessResult = new(InitializeSuccessResult);

        public ResultService()
        {
        }

        #region ERROR RESULTS
        public IErroredResult GetError(string? message)
        {
            return this.GetError(
                message,
                OperationType.Commit,
                null,
                null
            );
        }

        public IErroredResult GetError(string? message, OperationType operationType, string? property = null)
        {
            return this.GetError(
                message,
                operationType,
                null,
                property
            );
        }

        public IErroredResult GetError(Exception exception, string? property = null)
        {
            return this.GetError(
                Messages.OperationResult_DefaultError,
                OperationType.Commit,
                exception,
                null
            );
        }

        public IErroredResult GetError(string? message, OperationType operationType, Exception? exception, string? property = null)
        {
            ErrorDetails errorDetails = GetErrorDetails(exception, operationType, property);

            return new OperationResult
            {
                Message = GetErrorMessageOrDefault(message),
                Success = false,
                Error = errorDetails
            };
        }

        #endregion

        #region SUCCESS RESULTS
        public object GetQueryReply<T>(List<T> results, int limitedTo, IList<string>? propertiesRequested,
            string host, string? ldapFilter)
        {
            return new
            {
                Host = host,
                Request = new
                {
                    FilteredUsed = ldapFilter ?? string.Empty,
                    Limit = limitedTo,
                    PropertyCount = propertiesRequested?.Count ?? 0,
                    PropertiesRequested = propertiesRequested ?? Array.Empty<string>()
                },
                results.Count,
                Results = results
            };
        }

        public ISuccessResult GetSuccess()
        {
            VerifySuccessMessage(SuccessResult);

            return SuccessResult.Value;
        }

        #endregion

        #region BACKEND METHODS

        private static ErrorDetails GetErrorDetails(Exception? exception, OperationType type, string? property)
        {
            return exception is DirectoryServicesCOMException comException
                ? new ErrorDetails(comException)
                {
                    OperationType = type,
                    Property = property
                }
                : new ErrorDetails
                {
                    ErrorCode = exception?.HResult ?? int.MinValue,
                    ExtendedMessage = exception?.GetBaseException().Message,
                    OperationType = type,
                    Property = property
                };
        }

        private static string GetErrorMessageOrDefault(string? message)
        {
            return !string.IsNullOrWhiteSpace(message)
                ? message
                : Messages.OperationResult_DefaultError;
        }

        private static OperationResult InitializeSuccessResult()
        {
            return new()
            {
                Success = true,
                Message = Messages.OperationResult_Success
            };
        }

        private static void VerifySuccessMessage(Lazy<OperationResult> operation)
        {
            if (operation.IsValueCreated &&
                    (operation.Value.Message is null
                    ||
                    !operation.Value.Message.Equals(Messages.OperationResult_Success)
               ))
            {
                operation.Value.Message = Messages.OperationResult_Success;
            }
        }

        #endregion
    }
}