using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Password
{
    [ApiController]
    [Produces("application/json")]
    public class PasswordController : ADControllerBase
    {
        private IConnectionService Connections { get; }
        private IEncryptionService Encryption { get; }
        private IPasswordService Password { get; }
        private IResultService Results { get; }

        public PasswordController(IIdentityService identityService, IEncryptionService encryptionService,
            IPasswordService passwordService, IConnectionService connectionService, IResultService resultService)
            : base(identityService)
        {
            this.Connections = connectionService;
            this.Encryption = encryptionService;
            this.Password = passwordService;
            this.Results = resultService;
        }

        /// <summary>
        /// Changes an account's password using the current one and the supplied new one.
        /// </summary>
        /// <remarks>
        ///     This is different than 'resetting' as it requires the still valid, current password.
        /// </remarks>
        /// <param name="request">The password request from the request body.</param>
        /// <param name="domain">The domain or DC to contact for the request.</param>
        /// <returns>An object indicating the success of the operation.</returns>
        /// <status code="200">The password was successfully changed.</status>
        /// <status code="400">An exception occurred changing the password.</status>
        /// <status code="422">The request body did not supply the current account's password.</status>
        [HttpPut]
        [Route("password/change")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ISuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(IErroredResult))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ISuccessResult))]
        public IActionResult Change(
            [FromBody] PasswordRequest request,
            [FromQuery] string? domain = null)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return this.BadRequest(new
                {
                    Success = false,
                    Message = Messages.Error_CurrentPassIsRequired
                });
            }

            byte[] currentBytes;
            byte[] newBytes;

            if (request.IsEncrypted)
            {
                if (!this.Encryption.CanPerform)
                {
                    return this.UnprocessableEntity(new
                    {
                        Success = false,
                        Message = "The server is not configured to decrypt passwords."
                    });
                }

                currentBytes = this.Encryption.Decrypt(request.CurrentPassword);
                newBytes = this.Encryption.Decrypt(request.NewPassword);
            }
            else
            {
                currentBytes = Convert.FromBase64String(request.CurrentPassword);
                newBytes = Convert.FromBase64String(request.NewPassword);
            }

            using var connection = this.Connections.GetConnection(options =>
            {
                options.Domain = domain;
                options.Principal = this.HttpContext.User;
                options.DontDisposeHandle = true;
            });

            try
            {
                var result = this.Password.ChangePassword(request.DistinguishedName, connection, currentBytes, newBytes);
                return result.Success
                    ? this.Ok(result)
                    : this.BadRequest(result);
            }
            catch (Exception e)
            {
                return this.BadRequest(this.Results.GetError(e, "password"));
            }
            finally
            {
                Array.Clear(currentBytes, 0, currentBytes.Length);
                Array.Clear(newBytes, 0, newBytes.Length);
            }
        }

        /// <summary>
        /// Resets an account's password to the one supplied.
        /// </summary>
        /// <param name="request">The password request from the request body.</param>
        /// <param name="domain">The domain or DC to contact for the request.</param>
        /// <returns>An object indicating the success of the operation.</returns>
        [HttpPut]
        [Route("password/reset")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ISuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(IErroredResult))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ISuccessResult))]
        public IActionResult Reset(
            [FromBody] PasswordRequest request,
            [FromQuery] string? domain = null)
        {
            byte[] newBytes;
            if (request.IsEncrypted)
            {
                if (!this.Encryption.CanPerform)
                {
                    return this.UnprocessableEntity(new
                    {
                        Success = false,
                        Message = "The server is not configured to decrypt passwords."
                    });
                }

                newBytes = this.Encryption.Decrypt(request.NewPassword);
            }
            else
            {
                newBytes = Convert.FromBase64String(request.NewPassword);
            }

            using var connection = this.Connections.GetConnection(options =>
            {
                options.Domain = domain;
                options.Principal = this.HttpContext.User;
                options.DontDisposeHandle = false;
            });

            try
            {
                connection.GetRootDSE();
                var result = this.Password.SetPassword(request.DistinguishedName, connection, newBytes);
                return result.Success
                    ? this.Ok(result)
                    : this.BadRequest(result);
            }
            catch (Exception e)
            {
                return this.BadRequest(this.Results.GetError(e, "password"));
            }
            finally
            {
                Array.Clear(newBytes, 0, newBytes.Length);
            }
        }
    }
}
