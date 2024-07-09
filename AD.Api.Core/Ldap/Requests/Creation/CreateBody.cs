using AD.Api.Core.Ldap.Filters;
using Microsoft.AspNetCore.Http;
using System.Buffers;
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
        [MemberNotNullWhen(true, nameof(CommonName))]
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public bool HasPath { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public abstract FilteredRequestType RequestType { get; }

        private DistinguishedName? _dn = DistinguishedName.Empty;
        /// <inheritdoc/>
        public DistinguishedName GetDistinguishedName()
        {
            if (_dn.HasValue && _dn.Value.IsEmpty)
            {
                return _dn.Value;
            }

            if (!this.HasPath)
            {
                _dn = DistinguishedName.Parse(this.CommonName);
                return _dn.Value;
            }

            int count = DistinguishedName.CountNumberOfRelativeNames(this.CommonName) + 1;
            RelativeName[] array = ArrayPool<RelativeName>.Shared.Rent(count);
            Span<RelativeName> span = array.AsSpan(0, count);

            if (!DistinguishedName.TrySplit(this.CommonName, span.Slice(1), out int namesWritten)
                ||
                !RelativeName.TryParseOne(this.CommonName, out RelativeName first))
            {
                ArrayPool<RelativeName>.Shared.Return(array);
                return DistinguishedName.Empty;
            }

            span[0] = first;
            _dn = new(span.Slice(0, namesWritten + 1));
            ArrayPool<RelativeName>.Shared.Return(array);
            return _dn.Value;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return [];
        }
    }
}

