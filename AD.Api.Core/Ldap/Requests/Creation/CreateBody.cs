using AD.Api.Core.Ldap.Filters;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap
{
    /// <summary>
    /// Represents a request to create an LDAP object.
    /// </summary>
    public abstract class CreateBody : ICreateRequest, IServiceProvider, IValidatableObject
    {
        private string? _path;
        private IServiceProvider _requestSvc = null!;

        /// <summary>
        /// The specified common name (cn) for the object.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("cn")]
        [MinLength(1, ErrorMessage = "Common names should always be at least 1 character in length.")]
        public required string CommonName { get; set; }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Path))]
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool HasPath { get; private set; }
        /// <summary>
        /// The parent distinguished name of the container or organizational unit for the request.
        /// </summary>
        public string? Path
        {
            get => _path;
            set
            {
                _path = value;
                this.HasPath = !string.IsNullOrWhiteSpace(value);
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public abstract FilteredRequestType RequestType { get; }

        /// <inheritdoc/>
        public DistinguishedName GetDistinguishedName()
        {
            return new DistinguishedName(this.CommonName, this.Path);
        }

        public object? GetService(Type serviceType)
        {
            return _requestSvc?.GetService(serviceType);
        }

        public void SetRequestServices(HttpContext context)
        {
            _requestSvc = context.RequestServices;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return [];
        }
    }
}

