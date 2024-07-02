using AD.Api.Spans;
using AD.Api.Statics;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AD.Api.Core
{
    public readonly struct DomainQuery : IEquatable<DomainQuery>
    {
        public const string DomainModelName = "domain";
        public const string DomainControllerModelName = "dc";
        public const string DomainControllerFullModelName = "domainController";

        public readonly string Domain { get; }
        public readonly string? DomainController { get; }
        public readonly int UrlQueryLength { get; }

        public DomainQuery(string domain, string? domainController)
        {
            domain ??= string.Empty;
            this.Domain = domain;
            if (string.IsNullOrWhiteSpace(domainController))
            {
                this.DomainController = null;
                this.UrlQueryLength = DomainModelName.Length + domain.Length + 1;
            }
            else
            {
                this.DomainController = domainController;
                this.UrlQueryLength = DomainControllerModelName.Length
                                    + DomainModelName.Length
                                    + domain.Length
                                    + domainController.Length
                                    + 3;
            }
        }

        public bool Equals(DomainQuery other)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(this.Domain, other.Domain)
                && StringComparer.OrdinalIgnoreCase.Equals(this.DomainController, other.DomainController);
        }
        public override bool Equals(object? obj)
        {
            return obj is DomainQuery other && this.Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(this.Domain),
                StringComparer.OrdinalIgnoreCase.GetHashCode(this.DomainController ?? string.Empty));
        }

        public void AppendAsQuery(Span<char> destination, out int charsWritten)
        {
            int pos = 0;
            if (!string.IsNullOrWhiteSpace(this.Domain))
            {
                DomainModelName.CopyToSlice(destination, ref pos);
                destination[pos++] = CharConstants.EQUALS;
                this.Domain.CopyToSlice(destination, ref pos);
            }

            if (!string.IsNullOrWhiteSpace(this.DomainController))
            {
                if (pos > 0)
                {
                    destination[pos++] = CharConstants.AMP;
                }

                DomainControllerModelName.CopyToSlice(destination, ref pos);
                destination[pos++] = CharConstants.EQUALS;
                this.DomainController.CopyToSlice(destination, ref pos);
            }

            charsWritten = pos;
        }

        internal static ModelBindingResult Create(string domain, string? domainController, out DomainQuery result)
        {
            if (string.Empty != domain || domainController is not null)
            {
                result = new DomainQuery(domain, domainController);
                return ModelBindingResult.Success(result);
            }
            else
            {
                result = Default;
                return DefaultSuccess;
            }
        }

        public static readonly DomainQuery Default = new(string.Empty, null);
        internal static readonly ModelBindingResult DefaultSuccess = ModelBindingResult.Success(Default);

        public static bool operator ==(DomainQuery left, DomainQuery right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(DomainQuery left, DomainQuery right)
        {
            return !left.Equals(right);
        }
    }
}
