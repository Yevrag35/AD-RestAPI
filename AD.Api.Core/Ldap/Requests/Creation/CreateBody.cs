using AD.Api.Core.Ldap.Filters;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap
{
    /// <summary>
    /// Represents a request to create an LDAP object.
    /// </summary>
    public abstract class CreateBody : ICreateRequest, IValidatableObject
    {
        /// <summary>
        /// The specified common name (cn) for the object.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("cn")]
        [MinLength(1, ErrorMessage = "Common names should always be at least 1 character in length.")]
        public required string CommonName { get; init; }
        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Path))]
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool HasPath { get; private set; }
        /// <summary>
        /// The parent distinguished name of the container or organizational unit for the request.
        /// </summary>
        public string? Path { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public abstract FilteredRequestType RequestType { get; }

        /// <inheritdoc/>
        public DistinguishedName GetDistinguishedName()
        {
            
        }

        /// <inheritdoc/>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return [];
        }
    }
}

