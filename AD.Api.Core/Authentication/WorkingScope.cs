using AD.Api.Core.Authentication.Jwt;
using System.Runtime.InteropServices;

namespace AD.Api.Core.Authentication
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct WorkingScope : IEquatable<WorkingScope>
    {
        private readonly string? _dn;
        private readonly string? _domain;

        public readonly string DistinguishedName => _dn ?? string.Empty;
        public readonly string DomainName => _domain ?? string.Empty;
        public readonly AuthorizedRole RequiredRole { get; }

        public WorkingScope(string domainName, string distinguishedName, AuthorizedRole requiredRole)
        {
            _domain = domainName ?? string.Empty;
            _dn = distinguishedName;
            this.RequiredRole = requiredRole;
        }

        public readonly bool Equals(WorkingScope other)
        {
            return this.RequiredRole == other.RequiredRole
                && StringComparer.OrdinalIgnoreCase.Equals(this.DomainName, other.DomainName)
                && StringComparer.OrdinalIgnoreCase.Equals(this.DistinguishedName, other.DistinguishedName);
        }
        public readonly override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is WorkingScope other)
            {
                return this.Equals(other);
            }

            return false;
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(this.RequiredRole, 
                StringComparer.OrdinalIgnoreCase.GetHashCode(this.DomainName),
                StringComparer.OrdinalIgnoreCase.GetHashCode(this.DistinguishedName));
        }

        public static readonly WorkingScope Open = new(string.Empty, string.Empty, AuthorizedRole.None);

        public static bool operator ==(WorkingScope left, WorkingScope right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(WorkingScope left, WorkingScope right)
        {
            return !left.Equals(right);
        }
    }
}

