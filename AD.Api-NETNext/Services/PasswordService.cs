using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using AD.Api.Settings;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AD.Api.Services
{
    public interface IPasswordService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="connection"></param>
        /// <param name="currentPasswordBytes"></param>
        /// <param name="newPasswordBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        OperationResult ChangePassword(string distinguishedName, LdapConnection connection, byte[] currentPasswordBytes, byte[] newPasswordBytes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="connection"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        OperationResult SetPassword(string distinguishedName, LdapConnection connection, byte[] passwordBytes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirEntry"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        OperationResult SetPassword(DirectoryEntry dirEntry, byte[] password);
        bool TryGetFromBase64(string? base64Password, [NotNullWhen(true)] out byte[]? utf8Password);
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
        /// <param name="distinguishedName"></param>
        /// <param name="connection"></param>
        /// <param name="currentPasswordBytes"></param>
        /// <param name="newPasswordBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public OperationResult ChangePassword(string distinguishedName, LdapConnection connection, byte[] currentPasswordBytes, byte[] newPasswordBytes)
        {
            ArgumentNullException.ThrowIfNull(currentPasswordBytes);
            ArgumentNullException.ThrowIfNull(newPasswordBytes);

            if (!this.TryConvertPassword(currentPasswordBytes, out string? currentPassword, out OperationResult? result))
            {
                return result;
            }

            if (!this.TryConvertPassword(newPasswordBytes, out string? newPassword, out OperationResult? newResult))
            {
                return newResult;
            }

            using DirectoryEntry dirEntry = connection.GetDirectoryEntry(distinguishedName);

            try
            {
                connection.ExecuteInContext(() => dirEntry.Invoke(Strings.Invoke_ChangePassword, currentPassword, newPassword));
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
                        Property = "password"
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
                        Property = "password"
                    }
                };
            }
            finally
            {
                Array.Clear(currentPasswordBytes, 0, currentPasswordBytes.Length);
                Array.Clear(newPasswordBytes, 0, newPasswordBytes.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="connection"></param>
        /// <param name="passwordBytes">The plain-text bytes of the password.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="passwordBytes"/> is <see langword="null"/>.</exception>
        public OperationResult SetPassword(string distinguishedName, LdapConnection connection, byte[] passwordBytes)
        {
            using DirectoryEntry dirEntry = connection.GetDirectoryEntry(distinguishedName);
            using DirectoryEntry test = new DirectoryEntry("LDAP://GARVDC06.yevrag35.com:636/CN=Dat\\, Who X.,OU=Testing,OU=Real Users,DC=yevrag35,DC=com");
            dirEntry.UsePropertyCache = false;
            return this.SetPassword(connection, dirEntry, passwordBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirEntry"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public OperationResult SetPassword(DirectoryEntry dirEntry, byte[] password)
        {
            try
            {
                dirEntry.Invoke(Strings.Invoke_PasswordSet, password);
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
            finally
            {
                Array.Clear(password, 0, password.Length);
            }
        }

        public bool TryGetFromBase64(string? base64Password, [NotNullWhen(true)] out byte[]? utf8Password)
        {
            utf8Password = null;
            if (string.IsNullOrWhiteSpace(base64Password))
            {
                return false;
            }

            try
            {
                utf8Password = Convert.FromBase64String(base64Password);
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

        private bool TryConvertPassword(byte[] passwordBytes, [MaybeNullWhen(false)] out char[] passwordChars, [NotNullWhen(false)] out OperationResult? exception)
        {
            passwordChars = null;
            exception = null;

            if (passwordBytes is null || passwordBytes.Length <= 0)
            {
                var ex = new ArgumentException($"{nameof(passwordBytes)} cannot be null or empty.");
                exception = new OperationResult
                {
                    Success = false,
                    Message = "The password was null or empty.",
                    Error = new ErrorDetails
                    {
                        ErrorCode = ex.HResult,
                        ExtendedMessage = ex.Message,
                        OperationType = OperationType.Set,
                        Property = "password"
                    }
                };

                return false;
            }

            try
            {
                passwordChars = new char[passwordBytes.Length];

                int numberOfCharsCopied = this.TextOptions.Encoding.GetChars(passwordBytes, passwordChars);
                return numberOfCharsCopied == passwordBytes.Length && passwordChars.Length == passwordBytes.Length;
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

        [Obsolete($"Use the overload of {nameof(TryConvertPassword)} with the out char[] parameter.")]
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

        
        private OperationResult SetPassword(LdapConnection connection, DirectoryEntry dirEntry, byte[] password)
        {
            if (!this.TryConvertPassword(password, out string? passStr, out OperationResult? badResult))
            {
                return badResult;
            }

            try
            {
                dirEntry.Invoke(Strings.Invoke_PasswordSet, passStr);
                //connection.ExecuteInContext(() => dirEntry.Invoke(Strings.Invoke_PasswordSet, passStr));
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
            finally
            {
                Array.Clear(password, 0, password.Length);
            }
        }
    }
}
