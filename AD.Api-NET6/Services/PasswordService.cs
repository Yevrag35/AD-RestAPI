using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Operations;
using AD.Api.Settings;
using Microsoft.Extensions.Options;
using System.Configuration.Provider;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using Strings = AD.Api.Ldap.Properties.Resources;

namespace AD.Api.Services
{
    public interface IPasswordService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        OperationResult SetPassword(DirectoryEntry entry, byte[] passwordBytes);
        bool TryGetFromBase64(CreateOperationRequest request, [NotNullWhen(true)] out byte[]? utf8Password);
    }

    public class PasswordService : IPasswordService
    {
        private ITextSettings TextOptions { get; }

        public PasswordService(ITextSettings textSettings)
        {
            this.TextOptions = textSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public OperationResult SetPassword(DirectoryEntry entry, byte[] passwordBytes)
        {
            if (passwordBytes is null)
                throw new ArgumentNullException(nameof(passwordBytes));

            if (!this.TryConvertPassword(passwordBytes, out string? password, out OperationResult? result))
                return result;

            try
            {
                entry.Invoke(Strings.Invoke_PasswordSet, password);
                return new OperationResult
                {
                    Success = true,
                    Message = "Password updated successfully."
                };
            }
            catch (TargetInvocationException tie)
            {
                return HandleComException(tie);
            }
            catch (DirectoryServicesCOMException dsCom)
            {
                return new OperationResult
                {
                    Message = dsCom.Message,
                    Success = false,
                    Error = new ErrorDetails(dsCom)
                    {
                        OperationType = OperationType.Set,
                        Property = nameof(password)
                    }
                };
            }
            catch (Exception e)
            {
                return new OperationResult
                {
                    Message = e.Message,
                    Success = false,
                    Error = new ErrorDetails
                    {
                        ErrorCode = e.HResult,
                        ExtendedMessage = e.GetBaseException().Message,
                        OperationType = OperationType.Set,
                        Property = nameof(password)
                    }
                };
            }
        }

        public bool TryGetFromBase64(CreateOperationRequest request, [NotNullWhen(true)] out byte[]? utf8Password)
        {
            utf8Password = null;
            if (string.IsNullOrWhiteSpace(request.Base64Password))
                return false;

            try
            {
                utf8Password = Convert.FromBase64String(request.Base64Password);
            }
            catch
            {
                return false;
            }

            return utf8Password is not null && utf8Password.Length > 0;
        }


        private static OperationResult HandleComException(TargetInvocationException tie)
        {
            string? extendedError = null;
            int errorCode = tie.HResult;
            string message = tie.Message;

            if (tie.InnerException is COMException ce)
            {
                errorCode = ce.ErrorCode;
                extendedError = $"{ce.Message} {(ce.InnerException is not null ? ce.InnerException.Message : string.Empty)}";
                //Trace.TraceError(message);
                message = "error_not_found";

                // if the exception is due to password not meeting complexity requirements, 
                // then return ProviderException                
                if ((errorCode == unchecked((int)0x800708c5))
                    || (errorCode == unchecked((int)0x8007202f))
                    || (errorCode == unchecked((int)0x8007052d))
                    || (errorCode == unchecked((int)0x8007052f)))
                {
                    message = "password_not_complex";//password policy not met
                }
                else if ((errorCode == unchecked((int)0x8000500d)))
                {
                    message = "no_secure_conn_for_password";//need higher than security=None
                }
                else if ((errorCode == unchecked((int)0x80072035)))
                {
                    //min password length or the password was used before
                    //The server is unwilling to process the request
                    message = "min_password_length_used_before";
                }
                else if ((errorCode == unchecked((int)0x80005009)))
                {
                    message = "password_clear_text";//need to set option to use clear text
                }
            }

            return new OperationResult
            {
                Success = false,
                Message = message,
                Error = new ErrorDetails
                {
                    ErrorCode = errorCode,
                    ExtendedMessage = extendedError,
                    OperationType = OperationType.Set,
                    Property = "password"
                }
            };
        }
        private bool TryConvertPassword(byte[] passwordBytes, [NotNullWhen(true)] out string? password, [NotNullWhen(false)] out OperationResult? exception)
        {
            password = null;
            exception = null;
            try
            {
                password = this.TextOptions.Encoding.GetString(passwordBytes);
                return true;
            }
            catch (Exception e)
            {
                exception = new OperationResult
                {
                    Error = new ErrorDetails
                    {
                        ErrorCode = e.HResult,
                        ExtendedMessage = e.GetBaseException().Message,
                        OperationType = OperationType.Set,
                        Property = "password"
                    },
                    Message = "Unable to convert the password into a proper string.",
                    Success = false
                };
                return false;
            }
        }

    }
}
