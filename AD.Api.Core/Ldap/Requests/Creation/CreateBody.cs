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
        private DistinguishedName? _constructedDn;
        private IServiceProvider _requestSvc = null!;

        /// <summary>
        /// The specified common name (cn) for the object.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("cn")]
        [MinLength(1, ErrorMessage = "Common names should always be at least 1 character in length.")]
        public required string CommonName
        {
            get => _constructedDn?.CommonName ?? string.Empty;
            set
            {
                _constructedDn ??= new();
                _constructedDn.CommonName = value;
            }
        }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Path))]
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool HasPath { get; private set; }
        /// <summary>
        /// The parent distinguished name of the container or organizational unit for the request.
        /// </summary>
        public string? Path
        {
            get => _constructedDn?.Path ?? string.Empty;
            set
            {
                _constructedDn ??= new();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _constructedDn.Path = value;
                    this.HasPath = true;
                }
                else
                {
                    _constructedDn.Path = string.Empty;
                    this.HasPath = false;
                }
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public abstract FilteredRequestType RequestType { get; }

        /// <inheritdoc/>
        public DistinguishedName GetDistinguishedName()
        {
            return _constructedDn ??= new();
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

