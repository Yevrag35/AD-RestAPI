using AD.Api.Core.Ldap.Filters;

namespace AD.Api.Core.Ldap
{
    /// <summary>
    /// An interface representing a request to create an LDAP object.
    /// </summary>
    public interface ICreateRequest : IServiceProvider
    {
        /// <summary>
        /// Gets the type of request.
        /// </summary>
        FilteredRequestType RequestType { get; }
        /// <summary>
        /// Indicates whether the object has a parent path specified.
        /// </summary>
        bool HasPath { get; }

        /// <summary>
        /// Gets or constructs the distinguished name for the object.
        /// </summary>
        /// <returns>
        /// The distinguished name for the object.
        /// </returns>
        DistinguishedName GetDistinguishedName();
    }
}

