using AD.Api.Core.Ldap.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap
{
    /// <summary>
    /// Represents a request to create an LDAP object.
    /// </summary>
    public abstract class CreateBody : ICreateRequest, IValidatableObject
    {
        private string? _path;

        /// <summary>
        /// The specified common name (cn) for the object.
        /// </summary>
        [Required]
        [JsonPropertyName("cn")]
        [MinLength(1, ErrorMessage = "Common names should always be at least 1 character in length.")]
        public required string CommonName { get; set; }

        /// <inheritdoc/>
        [BindNever]
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

        public abstract IServiceProvider RequestServices { get; set; }

        [BindNever]
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public abstract FilteredRequestType RequestType { get; }

        /// <inheritdoc/>
        public DistinguishedName GetDistinguishedName()
        {
            return new DistinguishedName(this.CommonName, this.Path);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return [];
        }
    }
}

