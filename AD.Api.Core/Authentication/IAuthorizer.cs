using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AD.Api.Core.Authentication
{
    /// <summary>
    /// An authorization implementation for the AD API application depending on the authentication method used.
    /// </summary>
    public interface IAuthorizer
    {
        /// <summary>
        /// Authorizes the request based on the specified <see cref="AuthorizedRole"/> and sets the 
        /// context result to <see cref="ForbidResult"/> if the user is not authenticated or does not have the 
        /// required role.
        /// </summary>
        /// <remarks>
        /// The implementation logic depends on what type of authentication the API application is using. When
        /// NTLM/Kerberos/Negotiate is used, the implementation should do nothing.
        /// </remarks>
        /// <param name="context">The authorization context used by the filter.</param>
        /// <param name="role">The authorized role to check for.</param>
        void Authorize(AuthorizationFilterContext context, AuthorizedRole role);
        /// <summary>
        /// A second method to authorize the request based on the possible scoping rules that have been defined
        /// for the requester.
        /// </summary>
        /// <remarks>
        /// This method should always return <see langword="true"/> when using NTLM/Kerberos/Negotiate.
        /// </remarks>
        /// <param name="context">The <see cref="HttpContext"/> of the current request.</param>
        /// <param name="parentPath">The parent distinguishedName the request is attempting to query/manipulate.</param>
        /// <returns>
        /// <see langword="true"/> if the requester is further authorized to perform the request at the specified
        /// <paramref name="parentPath"/>; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsAuthorized(HttpContext context, string? parentPath);
    }
}

