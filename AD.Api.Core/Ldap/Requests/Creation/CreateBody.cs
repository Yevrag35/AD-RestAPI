using AD.Api.Core.Ldap.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap
{
    public abstract class CreateBody : ICreateRequest, IValidatableObject
    {
        private string? _path;

        [Required]
        [JsonPropertyName("cn")]
        [MinLength(1, ErrorMessage = "Common names should always be at least 1 character in length.")]
        public required string CommonName { get; set; }

        [BindNever]
        [MemberNotNullWhen(true, nameof(Path))]
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool HasPath { get; private set; }
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

        public DistinguishedName GetDistinguishedName()
        {
            return new DistinguishedName(this.CommonName, this.Path);
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return [];
        }
    }
}

